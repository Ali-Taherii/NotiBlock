using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using Microsoft.Extensions.Logging;

namespace NotiBlock.Backend.Services
{
    public class ResellerTicketService(
        AppDbContext context,
        ILogger<ResellerTicketService> logger,
        INotificationService notificationService) : IResellerTicketService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<ResellerTicketService> _logger = logger;
        private readonly INotificationService _notificationService = notificationService;

        public async Task<ResellerTicket> CreateTicketAsync(ResellerTicketCreateDTO dto, Guid resellerId)
        {
            // Check for duplicate open ticket with same category
            var exists = await _context.ResellerTickets
                .AnyAsync(t =>
                    t.ResellerId == resellerId &&
                    t.Category == dto.Category &&
                    t.Status == TicketStatus.Pending);

            if (exists)
            {
                _logger.LogWarning("Reseller {ResellerId} attempted to create duplicate ticket with category {Category}",
                    resellerId, dto.Category);
                throw new InvalidOperationException($"An open ticket with category '{dto.Category}' already exists");
            }

            var ticket = new ResellerTicket
            {
                ResellerId = resellerId,
                Category = dto.Category,
                Description = dto.Description,
                Priority = dto.Priority,
                Status = TicketStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.ResellerTickets.Add(ticket);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketId} created by reseller {ResellerId} with category {Category}",
                ticket.Id, resellerId, dto.Category);

            // ===== NOTIFICATIONS =====
            await SendTicketCreatedNotificationsAsync(ticket.Id, resellerId, dto.Category, dto.Priority);

            return ticket;
        }

        public async Task<ResellerTicket> GetTicketByIdAsync(Guid id)
        {
            var ticket = await _context.ResellerTickets
                .Include(t => t.Reseller)
                .Include(t => t.ApprovedBy)
                .Include(t => t.ConsumerReports)
                    .ThenInclude(r => r.Consumer)
                .Include(t => t.ConsumerReports)
                    .ThenInclude(r => r.Product)
                        .ThenInclude(p => p.Manufacturer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                _logger.LogWarning("Ticket not found: {TicketId}", id);
                throw new KeyNotFoundException($"Ticket with ID {id} not found");
            }

            return ticket;
        }

        public async Task<ResellerTicket> UpdateTicketAsync(Guid id, ResellerTicketUpdateDTO dto, Guid resellerId)
        {
            var ticket = await _context.ResellerTickets
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException($"Ticket with ID {id} not found");

            // Authorization: Only the reseller who created it can update
            if (ticket.ResellerId != resellerId)
            {
                _logger.LogWarning("Reseller {ResellerId} attempted to update ticket {TicketId} owned by reseller {OwnerId}",
                    resellerId, id, ticket.ResellerId);
                throw new UnauthorizedAccessException("You can only update your own tickets");
            }

            // Only allow updates if ticket is in Pending or Rejected status
            if (ticket.Status != TicketStatus.Pending && ticket.Status != TicketStatus.Rejected)
            {
                _logger.LogWarning("Attempted to update ticket {TicketId} with status {Status}",
                    id, ticket.Status);
                throw new InvalidOperationException($"Cannot update ticket with status '{ticket.Status}'");
            }

            var oldPriority = ticket.Priority;
            var priorityChanged = dto.Priority != oldPriority;

            // Update allowed fields
            ticket.Category = dto.Category;
            ticket.Description = dto.Description;
            ticket.Priority = dto.Priority;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketId} updated by reseller {ResellerId}",
                id, resellerId);

            // ===== NOTIFICATION (if priority increased) =====
            if (priorityChanged && dto.Priority > oldPriority)
            {
                await SendTicketPriorityIncreasedNotificationAsync(id, resellerId, oldPriority, dto.Priority);
            }

            return ticket;
        }

        public async Task<bool> DeleteTicketAsync(Guid id, Guid resellerId)
        {
            var ticket = await _context.ResellerTickets
                .IgnoreQueryFilters()
                .Include(t => t.ConsumerReports)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                _logger.LogWarning("Reseller {ResellerId} attempted to delete non-existent ticket: {TicketId}",
                    resellerId, id);
                throw new KeyNotFoundException($"Ticket with ID {id} not found");
            }

            // Check if already deleted
            if (ticket.IsDeleted)
            {
                _logger.LogWarning("Reseller {ResellerId} attempted to delete already deleted ticket: {TicketId}",
                    resellerId, id);
                throw new InvalidOperationException($"Ticket {id} is already deleted");
            }

            // Authorization: Only the reseller who created it can delete
            if (ticket.ResellerId != resellerId)
            {
                _logger.LogWarning("Reseller {ResellerId} attempted to delete ticket {TicketId} owned by reseller {OwnerId}",
                    resellerId, id, ticket.ResellerId);
                throw new UnauthorizedAccessException("You can only delete your own tickets");
            }

            // Only allow deletion if ticket is Pending or Rejected
            if (ticket.Status != TicketStatus.Pending && ticket.Status != TicketStatus.Rejected)
            {
                _logger.LogWarning("Attempted to delete ticket {TicketId} with status {Status}",
                    id, ticket.Status);
                throw new InvalidOperationException($"Cannot delete ticket with status '{ticket.Status}'");
            }

            // Perform soft delete
            ticket.IsDeleted = true;
            ticket.DeletedAt = DateTime.UtcNow;
            ticket.DeletedBy = resellerId;

            // Unlink any associated consumer reports
            var linkedReportIds = ticket.ConsumerReports?.Select(r => r.Id).ToList() ?? new List<Guid>();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketId} soft deleted by reseller {ResellerId} at {DeletedAt}",
                id, resellerId, ticket.DeletedAt);

            // ===== NOTIFICATIONS (notify consumers if reports were linked) =====
            if (linkedReportIds.Count > 0)
            {
                await SendTicketDeletedNotificationsAsync(id, linkedReportIds);
            }

            return true;
        }

        public async Task<PagedResultsDTO<ResellerTicket>> GetResellerTicketsAsync(Guid resellerId, int page, int pageSize)
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

            var query = _context.ResellerTickets
                .Include(t => t.ApprovedBy)
                .Include(t => t.ConsumerReports)
                .Where(t => t.ResellerId == resellerId)
                .OrderByDescending(t => t.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} tickets for reseller {ResellerId} (Page {Page} of {TotalPages})",
                items.Count, resellerId, page, Math.Ceiling(totalCount / (double)pageSize));

            return new PagedResultsDTO<ResellerTicket>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultsDTO<ResellerTicket>> GetAllTicketsAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.ResellerTickets
                .Include(t => t.Reseller)
                .Include(t => t.ApprovedBy)
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} tickets (Page {Page})", items.Count, page);

            return new PagedResultsDTO<ResellerTicket>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultsDTO<ResellerTicket>> GetTicketsByStatusAsync(TicketStatus status, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.ResellerTickets
                .Include(t => t.Reseller)
                .Include(t => t.ApprovedBy)
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} tickets with status {Status} (Page {Page})",
                items.Count, status, page);

            return new PagedResultsDTO<ResellerTicket>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<object> GetTicketStatisticsAsync(Guid? resellerId = null)
        {
            var query = _context.ResellerTickets.AsQueryable();

            if (resellerId.HasValue)
            {
                query = query.Where(t => t.ResellerId == resellerId.Value);
            }

            var stats = new
            {
                Total = await query.CountAsync(),
                Pending = await query.CountAsync(t => t.Status == TicketStatus.Pending),
                UnderReview = await query.CountAsync(t => t.Status == TicketStatus.UnderReview),
                Approved = await query.CountAsync(t => t.Status == TicketStatus.Approved),
                Rejected = await query.CountAsync(t => t.Status == TicketStatus.Rejected),
                Resolved = await query.CountAsync(t => t.Status == TicketStatus.Resolved),
                Closed = await query.CountAsync(t => t.Status == TicketStatus.Closed),
                ByCategory = await query
                    .GroupBy(t => t.Category)
                    .Select(g => new { Category = g.Key.ToString(), Count = g.Count() })
                    .ToListAsync(),
                ByPriority = await query
                    .GroupBy(t => t.Priority)
                    .Select(g => new { Priority = g.Key, Count = g.Count() })
                    .ToListAsync()
            };

            _logger.LogInformation("Ticket statistics retrieved for {Scope}",
                resellerId.HasValue ? $"reseller {resellerId}" : "all tickets");

            return stats;
        }

        public async Task<PagedResultsDTO<ResellerTicketReadableView>> GetReadableTicketsAsync(Guid? resellerId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.ResellerTicketsReadable.AsQueryable();

            if (resellerId.HasValue)
            {
                query = query.Where(t => t.ResellerId == resellerId.Value);
            }

            // Order by priority and creation date
            var orderedQuery = query
                .OrderByDescending(t => t.PriorityCode)
                .ThenByDescending(t => t.CreatedAt);

            var totalCount = await orderedQuery.CountAsync();
            var items = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} readable tickets (Page {Page})", items.Count, page);

            return new PagedResultsDTO<ResellerTicketReadableView>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // Link consumer reports to a reseller ticket
        public async Task<ResellerTicket> LinkConsumerReportsAsync(Guid ticketId, List<Guid> reportIds, Guid resellerId)
        {
            var ticket = await _context.ResellerTickets
                .Include(t => t.ConsumerReports)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
            {
                _logger.LogWarning("Reseller {ResellerId} attempted to link reports to non-existent ticket {TicketId}",
                    resellerId, ticketId);
                throw new KeyNotFoundException($"Ticket with ID {ticketId} not found");
            }

            // Authorization: Only the reseller who created the ticket can link reports
            if (ticket.ResellerId != resellerId)
            {
                _logger.LogWarning("Reseller {ResellerId} attempted to link reports to ticket {TicketId} owned by reseller {OwnerId}",
                    resellerId, ticketId, ticket.ResellerId);
                throw new UnauthorizedAccessException("You can only link reports to your own tickets");
            }

            // Only allow linking if ticket is Pending
            if (ticket.Status != TicketStatus.Pending)
            {
                _logger.LogWarning("Attempted to link reports to ticket {TicketId} with status {Status}",
                    ticketId, ticket.Status);
                throw new InvalidOperationException($"Cannot link reports to ticket with status '{ticket.Status}'. Only Pending tickets can be modified.");
            }

            // Get the reports
            var reports = await _context.ConsumerReports
                .Include(r => r.Product)
                .Where(r => reportIds.Contains(r.Id))
                .ToListAsync();

            if (reports.Count != reportIds.Count)
            {
                var foundIds = reports.Select(r => r.Id).ToList();
                var missingIds = reportIds.Except(foundIds).ToList();
                _logger.LogWarning("Some report IDs were not found: {MissingIds}", string.Join(", ", missingIds));
                throw new KeyNotFoundException($"Reports not found: {string.Join(", ", missingIds)}");
            }

            // Validate and link each report
            var linkedCount = 0;
            var errors = new List<string>();
            var linkedConsumerIds = new List<Guid>();

            foreach (var report in reports)
            {
                // Check if report is in valid status (Pending or UnderReview)
                if (report.Status != ReportStatus.Pending && report.Status != ReportStatus.UnderReview)
                {
                    errors.Add($"Report {report.Id} has status '{report.Status}' (must be Pending or UnderReview)");
                    continue;
                }

                // Check if already linked to another ticket
                if (report.ResellerTicketId.HasValue && report.ResellerTicketId != ticketId)
                {
                    errors.Add($"Report {report.Id} is already linked to ticket {report.ResellerTicketId}");
                    continue;
                }

                // Verify reseller sells the product
                if (report.Product == null)
                {
                    errors.Add($"Report {report.Id} has no associated product");
                    continue;
                }

                if (report.Product.ResellerId != resellerId)
                {
                    errors.Add($"Report {report.Id} is for a product you don't sell (Serial: {report.SerialNumber})");
                    continue;
                }

                // Link the report
                if (report.ResellerTicketId != ticketId)
                {
                    report.ResellerTicketId = ticketId;
                    report.Status = ReportStatus.EscalatedToReseller;
                    report.UpdatedAt = DateTime.UtcNow;
                    linkedCount++;
                    linkedConsumerIds.Add(report.ConsumerId);
                }
            }

            // If there were any errors, throw exception
            if (errors.Count != 0)
            {
                var errorMessage = string.Join("; ", errors);
                _logger.LogWarning("Failed to link some reports to ticket {TicketId}: {Errors}",
                    ticketId, errorMessage);
                throw new InvalidOperationException($"Failed to link some reports: {errorMessage}");
            }

            if (linkedCount == 0)
            {
                _logger.LogWarning("No reports were linked to ticket {TicketId} (all were already linked)",
                    ticketId);
                throw new InvalidOperationException("All specified reports are already linked to this ticket");
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Linked {Count} reports to ticket {TicketId} by reseller {ResellerId}",
                linkedCount, ticketId, resellerId);

            // ===== NOTIFICATIONS =====
            await SendReportsLinkedNotificationsAsync(ticketId, linkedConsumerIds.Distinct().ToList());

            return ticket;
        }

        // ===== NOTIFICATION HELPER METHODS =====

        private async Task SendTicketCreatedNotificationsAsync(Guid ticketId, Guid resellerId, TicketCategory category, int priority)
        {
            var notifications = new List<NotificationCreateDTO>();

            // 1. Notify reseller (confirmation)
            notifications.Add(new NotificationCreateDTO
            {
                RecipientId = resellerId,
                RecipientType = "reseller",
                Type = NotificationType.TicketCreated,
                Title = "Ticket Created Successfully",
                Message = $"Your {(priority == 3 ? "Critical" : priority == 2 ? "High" : "Normal")} priority ticket (Category: {category}) has been created and is pending review by regulators.",
                RelatedEntityId = ticketId,
                RelatedEntityType = "ticket",
                Priority = NotificationPriority.Normal
            });

            // 2. Notify all regulators
            var regulators = await _context.Regulators
                .Where(r => !r.IsDeleted)
                .Select(r => r.Id)
                .ToListAsync();

            var regulatorPriority = priority switch
            {
                3 => NotificationPriority.Critical,
                2 => NotificationPriority.High,
                _ => NotificationPriority.Normal
            };

            foreach (var regulatorId in regulators)
            {
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = regulatorId,
                    RecipientType = "regulator",
                    Type = NotificationType.TicketCreated,
                    Title = priority == 3 ? "CRITICAL: New Ticket Requires Review" : "New Ticket Created",
                    Message = $"A reseller has created a {(priority == 3 ? "Critical" : priority == 2 ? "High" : "Normal")} priority ticket (Category: {category}) that requires regulatory review.",
                    RelatedEntityId = ticketId,
                    RelatedEntityType = "ticket",
                    Priority = regulatorPriority
                });
            }

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Sent {Count} notifications for ticket {TicketId} creation",
                    notifications.Count, ticketId);
            }
        }

        private async Task SendTicketPriorityIncreasedNotificationAsync(Guid ticketId, Guid resellerId, int oldPriority, int newPriority)
        {
            var regulators = await _context.Regulators
                .Where(r => !r.IsDeleted)
                .Select(r => r.Id)
                .ToListAsync();

            var notifications = regulators.Select(regulatorId => new NotificationCreateDTO
            {
                RecipientId = regulatorId,
                RecipientType = "regulator",
                Type = NotificationType.Warning,
                Title = newPriority == 3 ? "CRITICAL: Ticket Priority Escalated" : "Ticket Priority Increased",
                Message = $"A ticket's priority has been increased from {(oldPriority == 2 ? "High" : "Normal")} to {(newPriority == 3 ? "Critical" : "High")}. This ticket requires immediate attention.",
                RelatedEntityId = ticketId,
                RelatedEntityType = "ticket",
                Priority = newPriority == 3 ? NotificationPriority.Critical : NotificationPriority.High
            }).ToList();

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Sent {Count} priority increase notifications for ticket {TicketId}",
                    notifications.Count, ticketId);
            }
        }

        private async Task SendTicketDeletedNotificationsAsync(Guid ticketId, List<Guid> linkedReportIds)
        {
            var consumerReports = await _context.ConsumerReports
                .Where(r => linkedReportIds.Contains(r.Id))
                .Select(r => new { r.ConsumerId, r.SerialNumber })
                .Distinct()
                .ToListAsync();

            var notifications = consumerReports.Select(report => new NotificationCreateDTO
            {
                RecipientId = report.ConsumerId,
                RecipientType = "consumer",
                Type = NotificationType.Info,
                Title = "Ticket Status Update",
                Message = $"The ticket associated with your product report ({report.SerialNumber}) has been withdrawn by the reseller. Your report status has been updated accordingly.",
                RelatedEntityId = ticketId,
                RelatedEntityType = "ticket",
                Priority = NotificationPriority.Normal
            }).ToList();

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Sent {Count} ticket deletion notifications", notifications.Count);
            }
        }

        private async Task SendReportsLinkedNotificationsAsync(Guid ticketId, List<Guid> consumerIds)
        {
            var notifications = consumerIds.Select(consumerId => new NotificationCreateDTO
            {
                RecipientId = consumerId,
                RecipientType = "consumer",
                Type = NotificationType.Info,
                Title = "Your Report Has Been Escalated",
                Message = $"Your product report has been escalated to a reseller ticket for regulatory review. You will be notified of any updates.",
                RelatedEntityId = ticketId,
                RelatedEntityType = "ticket",
                Priority = NotificationPriority.High
            }).ToList();

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Sent {Count} report linking notifications for ticket {TicketId}",
                    notifications.Count, ticketId);
            }
        }
    }
}
