using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services
{
    public class ConsumerReportService(
        AppDbContext context, 
        ILogger<ConsumerReportService> logger,
        INotificationService notificationService) : IConsumerReportService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<ConsumerReportService> _logger = logger;
        private readonly INotificationService _notificationService = notificationService;

        public async Task<ConsumerReport> SubmitReportAsync(ConsumerReportCreateDTO dto, Guid consumerId)
        {
            // Validate product exists - FIX: Use FirstOrDefaultAsync instead of FindAsync
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SerialNumber == dto.ProductSerialNumber.Trim());
            
            if (product == null)
            {
                _logger.LogWarning("Consumer {ConsumerId} attempted to report non-existent product {SerialNumber}",
                    consumerId, dto.ProductSerialNumber);
                throw new KeyNotFoundException($"Product with serial number {dto.ProductSerialNumber} not found");
            }

            // Validate consumer owns the product
            if (product.OwnerId != consumerId)
            {
                _logger.LogWarning("Consumer {ConsumerId} attempted to report product {SerialNumber} they don't own",
                    consumerId, dto.ProductSerialNumber);
                throw new UnauthorizedAccessException("You can only report products you own");
            }

            // Check for existing open report for this product
            var existingReport = await _context.ConsumerReports
                .AnyAsync(r => r.ConsumerId == consumerId && 
                              r.SerialNumber == dto.ProductSerialNumber.Trim() && 
                              r.Status == ReportStatus.Pending);

            if (existingReport)
            {
                _logger.LogWarning("Consumer {ConsumerId} attempted to create duplicate report for product {SerialNumber}",
                    consumerId, dto.ProductSerialNumber);
                throw new InvalidOperationException("You already have an open report for this product");
            }

            var report = new ConsumerReport
            {
                SerialNumber = dto.ProductSerialNumber.Trim().ToUpperInvariant(), // Normalize to uppercase
                Description = dto.IssueDescription.Trim(),
                ConsumerId = consumerId,
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.ConsumerReports.Add(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Report {ReportId} created by consumer {ConsumerId} for product {SerialNumber}",
                report.Id, consumerId, dto.ProductSerialNumber);

            // ===== NOTIFICATIONS =====
            await SendReportSubmittedNotificationsAsync(report.Id, consumerId, product);

            return report;
        }

        public async Task<ConsumerReport> GetReportByIdAsync(Guid id)
        {
            var report = await _context.ConsumerReports
                .Include(r => r.Consumer)
                .Include(r => r.Product)
                .Include(r => r.ResellerTicket)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                _logger.LogWarning("Report not found: {ReportId}", id);
                throw new KeyNotFoundException($"Report with ID {id} not found");
            }

            return report;
        }

        public async Task<ConsumerReport> UpdateReportAsync(Guid id, ConsumerReportUpdateDTO dto, Guid consumerId)
        {
            var report = await _context.ConsumerReports
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException($"Report with ID {id} not found");

            // Authorization: Only the consumer who created it can update
            if (report.ConsumerId != consumerId)
            {
                _logger.LogWarning("Consumer {ConsumerId} attempted to update report {ReportId} owned by consumer {OwnerId}",
                    consumerId, id, report.ConsumerId);
                throw new UnauthorizedAccessException("You can only update your own reports");
            }

            // Only allow updates if report is Pending
            if (report.Status != ReportStatus.Pending)
            {
                _logger.LogWarning("Attempted to update report {ReportId} with status {Status}",
                    id, report.Status);
                throw new InvalidOperationException($"Cannot update report with status '{report.Status}'");
            }

            report.Description = dto.IssueDescription.Trim();
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Report {ReportId} updated by consumer {ConsumerId}",
                id, consumerId);

            return report;
        }

        public async Task<bool> DeleteReportAsync(Guid id, Guid consumerId)
        {
            var report = await _context.ConsumerReports
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                _logger.LogWarning("Consumer {ConsumerId} attempted to delete non-existent report: {ReportId}",
                    consumerId, id);
                throw new KeyNotFoundException($"Report with ID {id} not found");
            }

            // Check if already deleted
            if (report.IsDeleted)
            {
                _logger.LogWarning("Consumer {ConsumerId} attempted to delete already deleted report: {ReportId}",
                    consumerId, id);
                throw new InvalidOperationException($"Report {id} is already deleted");
            }

            // Authorization: Only the consumer who created it can delete
            if (report.ConsumerId != consumerId)
            {
                _logger.LogWarning("Consumer {ConsumerId} attempted to delete report {ReportId} owned by consumer {OwnerId}",
                    consumerId, id, report.ConsumerId);
                throw new UnauthorizedAccessException("You can only delete your own reports");
            }

            // Only allow deletion if report is Pending
            if (report.Status != ReportStatus.Pending)
            {
                _logger.LogWarning("Attempted to delete report {ReportId} with status {Status}",
                    id, report.Status);
                throw new InvalidOperationException($"Cannot delete report with status '{report.Status}'");
            }

            // Perform soft delete
            report.IsDeleted = true;
            report.DeletedAt = DateTime.UtcNow;
            report.DeletedBy = consumerId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Report {ReportId} soft deleted by consumer {ConsumerId} at {DeletedAt}",
                id, consumerId, report.DeletedAt);

            return true;
        }

        public async Task<PagedResultsDTO<ConsumerReport>> GetConsumerReportsAsync(Guid consumerId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.ConsumerReports
                .Include(r => r.Product)
                .Include(r => r.ResellerTicket)
                .Where(r => r.ConsumerId == consumerId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} reports for consumer {ConsumerId} (Page {Page})",
                items.Count, consumerId, page);

            return new PagedResultsDTO<ConsumerReport>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultsDTO<ConsumerReport>> GetReportsByProductAsync(string serialNumber, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.ConsumerReports
                .Include(r => r.Consumer)
                .Include(r => r.ResellerTicket)
                .Where(r => r.SerialNumber == serialNumber)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} reports for product {SerialNumber} (Page {Page})",
                items.Count, serialNumber, page);

            return new PagedResultsDTO<ConsumerReport>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultsDTO<ConsumerReport>> GetAllReportsAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.ConsumerReports
                .Include(r => r.Consumer)
                .Include(r => r.Product)
                .Include(r => r.ResellerTicket)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} reports (Page {Page})", items.Count, page);

            return new PagedResultsDTO<ConsumerReport>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultsDTO<ConsumerReport>> GetReportsByStatusAsync(ReportStatus status, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.ConsumerReports
                .Include(r => r.Consumer)
                .Include(r => r.Product)
                .Include(r => r.ResellerTicket)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} reports with status {Status} (Page {Page})",
                items.Count, status, page);

            return new PagedResultsDTO<ConsumerReport>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ConsumerReport> ProcessReportActionAsync(Guid reportId, ConsumerReportActionDTO dto, Guid resellerId)
        {
            var report = await _context.ConsumerReports
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == reportId)
                ?? throw new KeyNotFoundException($"Report with ID {reportId} not found");

            // Validate reseller is associated with the product
            if (report.Product == null || report.Product.ResellerId != resellerId)
            {
                _logger.LogWarning("Reseller {ResellerId} attempted to process report {ReportId} for product not assigned to them",
                    resellerId, reportId);
                throw new UnauthorizedAccessException("You can only process reports for products assigned to you");
            }

            var oldStatus = report.Status;

            switch (dto.Action)
            {
                case ReportAction.Review:
                    if (report.Status != ReportStatus.Pending)
                    {
                        throw new InvalidOperationException($"Cannot review report with status '{report.Status}'");
                    }
                    report.Status = ReportStatus.UnderReview;
                    _logger.LogInformation("Report {ReportId} under review by reseller {ResellerId}",
                        reportId, resellerId);
                    break;

                case ReportAction.Resolve:
                    if (report.Status != ReportStatus.UnderReview && report.Status != ReportStatus.Pending)
                    {
                        throw new InvalidOperationException($"Cannot resolve report with status '{report.Status}'");
                    }
                    report.Status = ReportStatus.Resolved;
                    report.ResolvedBy = resellerId;
                    report.ResolvedAt = DateTime.UtcNow;
                    report.ResolutionNotes = dto.ResolutionNotes;
                    _logger.LogInformation("Report {ReportId} resolved by reseller {ResellerId}",
                        reportId, resellerId);
                    break;

                case ReportAction.RequestMoreInfo:
                    if (report.Status != ReportStatus.UnderReview)
                    {
                        throw new InvalidOperationException($"Cannot request more info for report with status '{report.Status}'");
                    }
                    report.Status = ReportStatus.UnderReview;
                    report.ResolutionNotes = dto.ResolutionNotes;
                    _logger.LogInformation("Reseller {ResellerId} requested more info for report {ReportId}",
                        resellerId, reportId);
                    break;

                case ReportAction.Escalate:
                    if (report.Status != ReportStatus.UnderReview && report.Status != ReportStatus.Pending)
                    {
                        throw new InvalidOperationException($"Cannot escalate report with status '{report.Status}'");
                    }
                    if (!dto.ResellerTicketId.HasValue)
                    {
                        throw new ArgumentException("ResellerTicketId is required for escalation");
                    }
                    
                    // Validate ticket exists and belongs to reseller
                    var ticket = await _context.ResellerTickets.FindAsync(dto.ResellerTicketId.Value);
                    if (ticket == null || ticket.ResellerId != resellerId)
                    {
                        throw new ArgumentException("Invalid reseller ticket");
                    }

                    report.Status = ReportStatus.EscalatedToReseller;
                    report.ResellerTicketId = dto.ResellerTicketId;
                    report.ResolutionNotes = dto.ResolutionNotes;
                    _logger.LogInformation("Report {ReportId} escalated to ticket {TicketId} by reseller {ResellerId}",
                        reportId, dto.ResellerTicketId, resellerId);
                    break;

                case ReportAction.Close:
                    if (report.Status != ReportStatus.Resolved)
                    {
                        throw new InvalidOperationException($"Cannot close report with status '{report.Status}'");
                    }
                    report.Status = ReportStatus.Closed;
                    _logger.LogInformation("Report {ReportId} closed by reseller {ResellerId}",
                        reportId, resellerId);
                    break;

                default:
                    throw new ArgumentException($"Invalid action: {dto.Action}");
            }

            report.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // ===== NOTIFICATIONS =====
            await SendReportStatusChangedNotificationsAsync(reportId, oldStatus, report.Status, dto.Action, resellerId);

            return report;
        }

        public async Task<object> GetReportStatisticsAsync(Guid? consumerId = null)
        {
            var query = _context.ConsumerReports.AsQueryable();

            if (consumerId.HasValue)
            {
                query = query.Where(r => r.ConsumerId == consumerId.Value);
            }

            var stats = new
            {
                Total = await query.CountAsync(),
                Pending = await query.CountAsync(r => r.Status == ReportStatus.Pending),
                UnderReview = await query.CountAsync(r => r.Status == ReportStatus.UnderReview),
                EscalatedToReseller = await query.CountAsync(r => r.Status == ReportStatus.EscalatedToReseller),
                Resolved = await query.CountAsync(r => r.Status == ReportStatus.Resolved),
                Closed = await query.CountAsync(r => r.Status == ReportStatus.Closed)
            };

            _logger.LogInformation("Report statistics retrieved for {Scope}",
                consumerId.HasValue ? $"consumer {consumerId}" : "all reports");

            return stats;
        }

        public async Task<PagedResultsDTO<ConsumerReportResponseDTO>> GetResellerRelatedReportsAsync(
            Guid resellerId, 
            int page, 
            int pageSize)
        {
            if (page < 1)
            {
                _logger.LogWarning("Invalid page number {Page} requested, defaulting to 1", page);
                page = 1;
            }

            if (pageSize < 1 || pageSize > 100)
            {
                _logger.LogWarning("Invalid page size {PageSize} requested, defaulting to 20", pageSize);
                pageSize = 20;
            }

            // Join ConsumerReports with Products to filter by ResellerId
            var query = from report in _context.ConsumerReports
                        join product in _context.Products on report.SerialNumber equals product.SerialNumber
                        where product.ResellerId == resellerId
                        orderby report.CreatedAt descending
                        select report;

            var totalCount = await query.CountAsync();
            
            // eagerly load Manufacturer safely
            var reports = await query
                .Include(r => r.Consumer)
                .Include(r => r.Product)
                    .ThenInclude(p => p.Manufacturer)
                .Include(r => r.ResellerTicket)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation(
                "Retrieved {Count} consumer reports for reseller {ResellerId} (Page {Page} of {TotalPages})",
                reports.Count, 
                resellerId, 
                page, 
                Math.Ceiling(totalCount / (double)pageSize)
            );

            return new PagedResultsDTO<ConsumerReportResponseDTO>
            {
                Items = [.. reports.Select(MapToResponseDTO)],
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ===== NOTIFICATION HELPER METHODS =====

        private async Task SendReportSubmittedNotificationsAsync(Guid reportId, Guid consumerId, Product product)
        {
            var notifications = new List<NotificationCreateDTO>();

            // 1. Notify consumer (confirmation)
            notifications.Add(new NotificationCreateDTO
            {
                RecipientId = consumerId,
                RecipientType = "consumer",
                Type = NotificationType.ReportSubmitted,
                Title = "Report Submitted Successfully",
                Message = $"Your report for product {product.SerialNumber} has been submitted and is pending review.",
                RelatedEntityId = reportId,
                RelatedEntityType = "report",
                Priority = NotificationPriority.Normal
            });

            // 2. Notify reseller if product is assigned to one
            if (product.ResellerId.HasValue)
            {
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = product.ResellerId.Value,
                    RecipientType = "reseller",
                    Type = NotificationType.ReportSubmitted,
                    Title = "New Consumer Report Received",
                    Message = $"A consumer has submitted a report for product {product.SerialNumber}. Please review and take appropriate action.",
                    RelatedEntityId = reportId,
                    RelatedEntityType = "report",
                    Priority = NotificationPriority.High
                });
            }

            // 3. Notify manufacturer (for awareness)
            notifications.Add(new NotificationCreateDTO
            {
                RecipientId = product.ManufacturerId,
                RecipientType = "manufacturer",
                Type = NotificationType.ReportSubmitted,
                Title = "Consumer Report Filed",
                Message = $"A consumer has reported an issue with product {product.SerialNumber} (Model: {product.Model}).",
                RelatedEntityId = reportId,
                RelatedEntityType = "report",
                Priority = NotificationPriority.Normal
            });

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Sent {Count} notifications for report {ReportId} submission",
                    notifications.Count, reportId);
            }
        }

        private async Task SendReportStatusChangedNotificationsAsync(
            Guid reportId, 
            ReportStatus oldStatus, 
            ReportStatus newStatus, 
            ReportAction action,
            Guid resellerId)
        {
            var report = await _context.ConsumerReports
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null) return;

            var notifications = new List<NotificationCreateDTO>();

            // Determine message based on action
            var (title, message, priority) = action switch
            {
                ReportAction.Review => (
                    "Report Under Review",
                    $"Your report for product {report.SerialNumber} is now being reviewed by the reseller.",
                    NotificationPriority.Normal
                ),
                ReportAction.Resolve => (
                    "Report Resolved",
                    $"Your report for product {report.SerialNumber} has been resolved. Resolution: {report.ResolutionNotes}",
                    NotificationPriority.High
                ),
                ReportAction.RequestMoreInfo => (
                    "Additional Information Requested",
                    $"The reseller has requested more information about your report for product {report.SerialNumber}. Please check the resolution notes.",
                    NotificationPriority.High
                ),
                ReportAction.Escalate => (
                    "Report Escalated",
                    $"Your report for product {report.SerialNumber} has been escalated for further investigation.",
                    NotificationPriority.High
                ),
                ReportAction.Close => (
                    "Report Closed",
                    $"Your report for product {report.SerialNumber} has been closed.",
                    NotificationPriority.Normal
                ),
                _ => (
                    "Report Status Updated",
                    $"Your report for product {report.SerialNumber} status has been updated to {newStatus}.",
                    NotificationPriority.Normal
                )
            };

            // Notify consumer about status change
            notifications.Add(new NotificationCreateDTO
            {
                RecipientId = report.ConsumerId,
                RecipientType = "consumer",
                Type = NotificationType.Info,
                Title = title,
                Message = message,
                RelatedEntityId = reportId,
                RelatedEntityType = "report",
                Priority = priority
            });

            // If escalated, notify manufacturer
            if (action == ReportAction.Escalate && report.Product != null)
            {
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = report.Product.ManufacturerId,
                    RecipientType = "manufacturer",
                    Type = NotificationType.Warning,
                    Title = "Consumer Report Escalated",
                    Message = $"A consumer report for product {report.SerialNumber} has been escalated by the reseller for your attention.",
                    RelatedEntityId = reportId,
                    RelatedEntityType = "report",
                    Priority = NotificationPriority.High
                });
            }

            // If resolved, also notify manufacturer
            if (action == ReportAction.Resolve && report.Product != null)
            {
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = report.Product.ManufacturerId,
                    RecipientType = "manufacturer",
                    Type = NotificationType.Info,
                    Title = "Consumer Report Resolved",
                    Message = $"A consumer report for product {report.SerialNumber} has been resolved by the reseller.",
                    RelatedEntityId = reportId,
                    RelatedEntityType = "report",
                    Priority = NotificationPriority.Normal
                });
            }

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Sent {Count} notifications for report {ReportId} status change from {OldStatus} to {NewStatus}",
                    notifications.Count, reportId, oldStatus, newStatus);
            }
        }

        private ConsumerReportResponseDTO MapToResponseDTO(ConsumerReport report)
        {
            return new ConsumerReportResponseDTO
            {
                Id = report.Id,
                ConsumerId = report.ConsumerId,
                ConsumerName = report.Consumer?.Name ?? "Unknown",
                ConsumerEmail = report.Consumer?.Email ?? string.Empty,
                SerialNumber = report.SerialNumber,
                
                // Add product information
                Product = report.Product != null ? new ProductBasicInfoDTO
                {
                    Id = report.Product.Id,
                    SerialNumber = report.Product.SerialNumber,
                    Model = report.Product.Model,
                    ManufacturerName = report.Product.Manufacturer?.CompanyName ?? "Unknown"
                } : null,
                
                Description = report.Description,
                Status = report.Status,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt,
                ResolvedAt = report.ResolvedAt,
                ResellerTicketId = report.ResellerTicketId,
                ResolvedBy = report.ResolvedBy,
                ResolutionNotes = report.ResolutionNotes
            };
        }
    }
}
