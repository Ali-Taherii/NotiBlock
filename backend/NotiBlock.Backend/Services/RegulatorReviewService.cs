using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services
{
    public class RegulatorReviewService(AppDbContext context, ILogger<RegulatorReviewService> logger, INotificationService notificationService) : IRegulatorReviewService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<RegulatorReviewService> _logger = logger;
        private readonly INotificationService _notificationService = notificationService;

        public async Task<RegulatorReview> CreateReviewAsync(RegulatorReviewCreateDTO dto, Guid regulatorId)
        {
            // Validate ticket exists
            var ticket = await _context.ResellerTickets
                .Include(t => t.Reseller)
                .Include(t => t.ConsumerReports)
                .FirstOrDefaultAsync(t => t.Id == dto.TicketId);

            if (ticket == null)
            {
                _logger.LogWarning("Regulator {RegulatorId} attempted to review non-existent ticket {TicketId}",
                    regulatorId, dto.TicketId);
                throw new KeyNotFoundException($"Ticket with ID {dto.TicketId} not found");
            }

            // Only allow review if ticket is Pending or UnderReview
            if (ticket.Status != TicketStatus.Pending && ticket.Status != TicketStatus.UnderReview)
            {
                _logger.LogWarning("Attempted to review ticket {TicketId} with status {Status}",
                    dto.TicketId, ticket.Status);
                throw new InvalidOperationException($"Cannot review ticket with status '{ticket.Status}'");
            }

            // Check for existing review by this regulator
            var existingReview = await _context.RegulatorReviews
                .AnyAsync(r => r.TicketId == dto.TicketId && r.RegulatorId == regulatorId);

            if (existingReview)
            {
                _logger.LogWarning("Regulator {RegulatorId} attempted to create duplicate review for ticket {TicketId}",
                    regulatorId, dto.TicketId);
                throw new InvalidOperationException("You have already reviewed this ticket. Use update instead.");
            }

            var review = new RegulatorReview
            {
                TicketId = dto.TicketId,
                RegulatorId = regulatorId,
                Decision = dto.Decision,
                Notes = dto.Notes.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.RegulatorReviews.Add(review);

            var oldStatus = ticket.Status;

            // Update ticket status based on decision
            switch (dto.Decision)
            {
                case ReviewDecision.Approved:
                    ticket.Status = TicketStatus.Approved;
                    ticket.ApprovedById = regulatorId;
                    ticket.UpdatedAt = DateTime.UtcNow;
                    break;

                case ReviewDecision.Rejected:
                    ticket.Status = TicketStatus.Rejected;
                    ticket.ApprovedById = regulatorId;
                    ticket.ResolutionNotes = dto.Notes;
                    ticket.UpdatedAt = DateTime.UtcNow;
                    break;

                case ReviewDecision.NeedsMoreInfo:
                    ticket.Status = TicketStatus.UnderReview;
                    ticket.UpdatedAt = DateTime.UtcNow;
                    break;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Review {ReviewId} created by regulator {RegulatorId} for ticket {TicketId} with decision {Decision}",
                review.Id, regulatorId, dto.TicketId, dto.Decision);

            // ===== NOTIFICATIONS =====
            await SendReviewCreatedNotificationsAsync(dto.TicketId, regulatorId, dto.Decision, dto.Notes, ticket.ResellerId);

            return review;
        }

        public async Task<RegulatorReview> GetReviewByIdAsync(Guid id)
        {
            var review = await _context.RegulatorReviews
                .Include(r => r.Ticket)
                    .ThenInclude(t => t.Reseller)
                .Include(r => r.Ticket)
                    .ThenInclude(t => t.ConsumerReports)
                .Include(r => r.Regulator)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                _logger.LogWarning("Review not found: {ReviewId}", id);
                throw new KeyNotFoundException($"Review with ID {id} not found");
            }

            return review;
        }

        public async Task<RegulatorReview> UpdateReviewAsync(Guid id, RegulatorReviewUpdateDTO dto, Guid regulatorId)
        {
            var review = await _context.RegulatorReviews
                .Include(r => r.Ticket)
                    .ThenInclude(t => t.Reseller)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException($"Review with ID {id} not found");

            // Authorization: Only the regulator who created it can update
            if (review.RegulatorId != regulatorId)
            {
                _logger.LogWarning("Regulator {RegulatorId} attempted to update review {ReviewId} created by regulator {OwnerId}",
                    regulatorId, id, review.RegulatorId);
                throw new UnauthorizedAccessException("You can only update your own reviews");
            }

            // Only allow updates if ticket is not already Resolved or Closed
            if (review.Ticket.Status == TicketStatus.Resolved || review.Ticket.Status == TicketStatus.Closed)
            {
                _logger.LogWarning("Attempted to update review {ReviewId} for ticket with status {Status}",
                    id, review.Ticket.Status);
                throw new InvalidOperationException($"Cannot update review for ticket with status '{review.Ticket.Status}'");
            }

            var oldDecision = review.Decision;
            review.Decision = dto.Decision;
            review.Notes = dto.Notes.Trim();

            // Update ticket status if decision changed
            if (oldDecision != dto.Decision)
            {
                switch (dto.Decision)
                {
                    case ReviewDecision.Approved:
                        review.Ticket.Status = TicketStatus.Approved;
                        review.Ticket.ApprovedById = regulatorId;
                        break;

                    case ReviewDecision.Rejected:
                        review.Ticket.Status = TicketStatus.Rejected;
                        review.Ticket.ResolutionNotes = dto.Notes;
                        break;

                    case ReviewDecision.NeedsMoreInfo:
                        review.Ticket.Status = TicketStatus.UnderReview;
                        break;
                }

                review.Ticket.UpdatedAt = DateTime.UtcNow;

                // ===== NOTIFICATION (only if decision changed) =====
                await SendReviewUpdatedNotificationsAsync(review.Ticket.Id, regulatorId, oldDecision, dto.Decision, dto.Notes, review.Ticket.ResellerId);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Review {ReviewId} updated by regulator {RegulatorId}",
                id, regulatorId);

            return review;
        }

        public async Task<bool> DeleteReviewAsync(Guid id, Guid regulatorId)
        {
            var review = await _context.RegulatorReviews
                .IgnoreQueryFilters()
                .Include(r => r.Ticket)
                    .ThenInclude(t => t.Reseller)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                _logger.LogWarning("Regulator {RegulatorId} attempted to delete non-existent review: {ReviewId}",
                    regulatorId, id);
                throw new KeyNotFoundException($"Review with ID {id} not found");
            }

            // Check if already deleted
            if (review.IsDeleted)
            {
                _logger.LogWarning("Regulator {RegulatorId} attempted to delete already deleted review: {ReviewId}",
                    regulatorId, id);
                throw new InvalidOperationException($"Review {id} is already deleted");
            }

            // Authorization: Only the regulator who created it can delete
            if (review.RegulatorId != regulatorId)
            {
                _logger.LogWarning("Regulator {RegulatorId} attempted to delete review {ReviewId} created by regulator {OwnerId}",
                    regulatorId, id, review.RegulatorId);
                throw new UnauthorizedAccessException("You can only delete your own reviews");
            }

            // Only allow deletion if ticket is not Resolved or Closed
            if (review.Ticket.Status == TicketStatus.Resolved || review.Ticket.Status == TicketStatus.Closed)
            {
                _logger.LogWarning("Attempted to delete review {ReviewId} for ticket with status {Status}",
                    id, review.Ticket.Status);
                throw new InvalidOperationException($"Cannot delete review for ticket with status '{review.Ticket.Status}'");
            }

            // Perform soft delete
            review.IsDeleted = true;
            review.DeletedAt = DateTime.UtcNow;
            review.DeletedBy = regulatorId;

            // Revert ticket to Pending status
            review.Ticket.Status = TicketStatus.Pending;
            review.Ticket.ApprovedById = null;
            review.Ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Review {ReviewId} soft deleted by regulator {RegulatorId} at {DeletedAt}",
                id, regulatorId, review.DeletedAt);

            // ===== NOTIFICATION =====
            await _notificationService.CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = review.Ticket.ResellerId,
                RecipientType = "reseller",
                Type = NotificationType.Info,
                Title = "Review Deleted - Ticket Status Reverted",
                Message = $"A regulator has deleted their review for your ticket. The ticket status has been reverted to Pending for re-evaluation.",
                RelatedEntityId = review.Ticket.Id,
                RelatedEntityType = "ticket",
                Priority = NotificationPriority.High
            });

            return true;
        }

        public async Task<PagedResultsDTO<RegulatorReview>> GetReviewsByRegulatorAsync(Guid regulatorId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.RegulatorReviews
                .Include(r => r.Ticket)
                    .ThenInclude(t => t.Reseller)
                .Where(r => r.RegulatorId == regulatorId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} reviews for regulator {RegulatorId} (Page {Page})",
                items.Count, regulatorId, page);

            return new PagedResultsDTO<RegulatorReview>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultsDTO<RegulatorReview>> GetReviewsByTicketAsync(Guid ticketId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.RegulatorReviews
                .Include(r => r.Regulator)
                .Where(r => r.TicketId == ticketId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} reviews for ticket {TicketId} (Page {Page})",
                items.Count, ticketId, page);

            return new PagedResultsDTO<RegulatorReview>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultsDTO<ResellerTicket>> GetPendingTicketsAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.ResellerTickets
                .Include(t => t.Reseller)
                .Include(t => t.ConsumerReports)
                .Where(t => t.Status == TicketStatus.Pending || t.Status == TicketStatus.UnderReview)
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} pending tickets for review (Page {Page})",
                items.Count, page);

            return new PagedResultsDTO<ResellerTicket>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ResellerTicket> EscalateToManufacturerAsync(Guid ticketId, Guid regulatorId)
        {
            var ticket = await _context.ResellerTickets
                .Include(t => t.RegulatorReviews)
                .Include(t => t.ConsumerReports)
                    .ThenInclude(cr => cr.Product)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
            {
                _logger.LogWarning("Regulator {RegulatorId} attempted to escalate non-existent ticket {TicketId}",
                    regulatorId, ticketId);
                throw new KeyNotFoundException($"Ticket with ID {ticketId} not found");
            }

            // Only Approved tickets can be escalated
            if (ticket.Status != TicketStatus.Approved)
            {
                _logger.LogWarning("Attempted to escalate ticket {TicketId} with status {Status}",
                    ticketId, ticket.Status);
                throw new InvalidOperationException($"Only Approved tickets can be escalated. Current status: '{ticket.Status}'");
            }

            // Verify regulator has approved this ticket
            var hasApproved = ticket.RegulatorReviews.Any(r => r.RegulatorId == regulatorId && r.Decision == ReviewDecision.Approved);
            
            if (!hasApproved && ticket.ApprovedById != regulatorId)
            {
                _logger.LogWarning("Regulator {RegulatorId} attempted to escalate ticket {TicketId} they didn't approve",
                    regulatorId, ticketId);
                throw new UnauthorizedAccessException("You can only escalate tickets you have approved");
            }

            // Update ticket status
            ticket.Status = TicketStatus.UnderReview;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketId} escalated to manufacturer by regulator {RegulatorId}",
                ticketId, regulatorId);

            // ===== NOTIFICATIONS =====
            await SendTicketEscalatedNotificationsAsync(ticketId, regulatorId, ticket);

            return ticket;
        }

        public async Task<object> GetRegulatorStatsAsync(Guid regulatorId)
        {
            var totalReviews = await _context.RegulatorReviews
                .CountAsync(r => r.RegulatorId == regulatorId);

            var approvedCount = await _context.RegulatorReviews
                .CountAsync(r => r.RegulatorId == regulatorId && r.Decision == ReviewDecision.Approved);

            var rejectedCount = await _context.RegulatorReviews
                .CountAsync(r => r.RegulatorId == regulatorId && r.Decision == ReviewDecision.Rejected);

            var needsMoreInfoCount = await _context.RegulatorReviews
                .CountAsync(r => r.RegulatorId == regulatorId && r.Decision == ReviewDecision.NeedsMoreInfo);

            var pendingTicketsCount = await _context.ResellerTickets
                .CountAsync(t => t.Status == TicketStatus.Pending || t.Status == TicketStatus.UnderReview);

            _logger.LogInformation("Stats retrieved for regulator {RegulatorId}", regulatorId);

            return new
            {
                totalReviews,
                approved = approvedCount,
                rejected = rejectedCount,
                needsMoreInfo = needsMoreInfoCount,
                pendingTickets = pendingTicketsCount,
                approvalRate = totalReviews > 0 ? Math.Round((double)approvedCount / totalReviews * 100, 2) : 0
            };
        }

        public async Task<PagedResultsDTO<ResellerTicket>> GetApprovedTicketsForManufacturerAsync(Guid manufacturerId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            // Get approved tickets that have consumer reports for this manufacturer's products
            var query = from ticket in _context.ResellerTickets
                        join report in _context.ConsumerReports on ticket.Id equals report.ResellerTicketId
                        join product in _context.Products on report.SerialNumber equals product.SerialNumber
                        where product.ManufacturerId == manufacturerId
                              && ticket.Status == TicketStatus.Approved
                              && !ticket.IsDeleted
                        select ticket;

            // Distinct tickets (a ticket might have multiple reports for the same manufacturer)
            var distinctQuery = query
                .Distinct()
                .Include(t => t.Reseller)
                .Include(t => t.ApprovedBy)
                .Include(t => t.ConsumerReports)
                    .ThenInclude(cr => cr.Product)
                .Include(t => t.RegulatorReviews)
                .OrderByDescending(t => t.UpdatedAt);

            var totalCount = await distinctQuery.CountAsync();
            var items = await distinctQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} approved tickets for manufacturer {ManufacturerId} (Page {Page})",
                items.Count, manufacturerId, page);

            return new PagedResultsDTO<ResellerTicket>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ===== NOTIFICATION HELPER METHODS =====

        private async Task SendReviewCreatedNotificationsAsync(
            Guid ticketId,
            Guid regulatorId,
            ReviewDecision decision,
            string notes,
            Guid resellerId)
        {
            var notifications = new List<NotificationCreateDTO>();

            // Determine message based on decision
            var (title, message, priority, notificationType) = decision switch
            {
                ReviewDecision.Approved => (
                    "Ticket Approved by Regulator",
                    $"Your ticket has been approved by a regulator. The ticket can now be escalated to the manufacturer for further action.",
                    NotificationPriority.High,
                    NotificationType.TicketApproved
                ),
                ReviewDecision.Rejected => (
                    "Ticket Rejected by Regulator",
                    $"Your ticket has been rejected by a regulator. Reason: {notes}. Please review and address the feedback.",
                    NotificationPriority.High,
                    NotificationType.TicketRejected
                ),
                ReviewDecision.NeedsMoreInfo => (
                    "Additional Information Required for Ticket",
                    $"A regulator has requested additional information for your ticket. Please review: {notes}",
                    NotificationPriority.High,
                    NotificationType.Info
                ),
                _ => (
                    "Ticket Reviewed by Regulator",
                    $"Your ticket has been reviewed by a regulator.",
                    NotificationPriority.Normal,
                    NotificationType.ReviewSubmitted
                )
            };

            // Notify reseller
            notifications.Add(new NotificationCreateDTO
            {
                RecipientId = resellerId,
                RecipientType = "reseller",
                Type = notificationType,
                Title = title,
                Message = message,
                RelatedEntityId = ticketId,
                RelatedEntityType = "ticket",
                Priority = priority
            });

            // Get affected consumers from related reports and notify them
            var consumerReports = await _context.ConsumerReports
                .Where(cr => cr.ResellerTicketId == ticketId)
                .Select(cr => new { cr.ConsumerId, cr.SerialNumber })
                .Distinct()
                .ToListAsync();

            foreach (var report in consumerReports)
            {
                var consumerMessage = decision switch
                {
                    ReviewDecision.Approved => $"The ticket related to your product report ({report.SerialNumber}) has been approved by regulators and is being escalated for resolution.",
                    ReviewDecision.Rejected => $"The ticket related to your product report ({report.SerialNumber}) has been rejected. The reseller will be notified to address your concerns directly.",
                    ReviewDecision.NeedsMoreInfo => $"Additional information is being requested for the ticket related to your product report ({report.SerialNumber}). You may be contacted for further details.",
                    _ => $"The ticket related to your product report ({report.SerialNumber}) has been reviewed by regulators."
                };

                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = report.ConsumerId,
                    RecipientType = "consumer",
                    Type = NotificationType.Info,
                    Title = "Update on Your Product Report",
                    Message = consumerMessage,
                    RelatedEntityId = ticketId,
                    RelatedEntityType = "ticket",
                    Priority = NotificationPriority.Normal
                });
            }

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Sent {Count} notifications for review creation on ticket {TicketId}",
                    notifications.Count, ticketId);
            }
        }

        private async Task SendReviewUpdatedNotificationsAsync(
            Guid ticketId,
            Guid regulatorId,
            ReviewDecision oldDecision,
            ReviewDecision newDecision,
            string notes,
            Guid resellerId)
        {
            var notifications = new List<NotificationCreateDTO>();

            // Notify reseller
            var (title, message, priority) = newDecision switch
            {
                ReviewDecision.Approved => (
                    "Ticket Review Updated - Now Approved",
                    $"A regulator has updated their review for your ticket from {oldDecision} to Approved. The ticket can now be escalated to the manufacturer.",
                    NotificationPriority.High
                ),
                ReviewDecision.Rejected => (
                    "Ticket Review Updated - Now Rejected",
                    $"A regulator has updated their review for your ticket from {oldDecision} to Rejected. Reason: {notes}",
                    NotificationPriority.High
                ),
                ReviewDecision.NeedsMoreInfo => (
                    "Ticket Review Updated - More Information Needed",
                    $"A regulator has updated their review for your ticket. Additional information is required: {notes}",
                    NotificationPriority.High
                ),
                _ => (
                    "Ticket Review Updated",
                    $"A regulator has updated their review for your ticket from {oldDecision} to {newDecision}.",
                    NotificationPriority.Normal
                )
            };

            notifications.Add(new NotificationCreateDTO
            {
                RecipientId = resellerId,
                RecipientType = "reseller",
                Type = NotificationType.Info,
                Title = title,
                Message = message,
                RelatedEntityId = ticketId,
                RelatedEntityType = "ticket",
                Priority = priority
            });

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Sent {Count} notifications for review update on ticket {TicketId}",
                    notifications.Count, ticketId);
            }
        }

        private async Task SendTicketEscalatedNotificationsAsync(Guid ticketId, Guid regulatorId, ResellerTicket ticket)
        {
            var notifications = new List<NotificationCreateDTO>();

            // Get unique manufacturer IDs from consumer reports
            var manufacturerIds = await _context.ConsumerReports
                .Where(cr => cr.ResellerTicketId == ticketId)
                .Include(cr => cr.Product)
                .Select(cr => cr.Product!.ManufacturerId)
                .Distinct()
                .ToListAsync();

            // Notify manufacturers
            foreach (var manufacturerId in manufacturerIds)
            {
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = manufacturerId,
                    RecipientType = "manufacturer",
                    Type = NotificationType.Warning,
                    Title = "Ticket Escalated to You by Regulator",
                    Message = $"A {ticket.Priority} priority ticket (Category: {ticket.Category}) has been escalated to you by regulators for immediate attention and resolution. Please review the associated consumer reports.",
                    RelatedEntityId = ticketId,
                    RelatedEntityType = "ticket",
                    Priority = ticket.Priority switch
                    {
                        2 => NotificationPriority.High,
                        3 => NotificationPriority.Critical,
                        _ => NotificationPriority.Normal

                    }
                });
            }

            // Notify reseller
            notifications.Add(new NotificationCreateDTO
            {
                RecipientId = ticket.ResellerId,
                RecipientType = "reseller",
                Type = NotificationType.Info,
                Title = "Your Ticket Has Been Escalated",
                Message = $"Your ticket (Category: {ticket.Category}) has been escalated to the manufacturer by a regulator. You will be notified of any updates.",
                RelatedEntityId = ticketId,
                RelatedEntityType = "ticket",
                Priority = NotificationPriority.Normal
            });

            // Notify affected consumers
            var consumerReports = await _context.ConsumerReports
                .Where(cr => cr.ResellerTicketId == ticketId)
                .Select(cr => new { cr.ConsumerId, cr.SerialNumber })
                .Distinct()
                .ToListAsync();

            foreach (var report in consumerReports)
            {
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = report.ConsumerId,
                    RecipientType = "consumer",
                    Type = NotificationType.Info,
                    Title = "Your Report Has Been Escalated",
                    Message = $"The ticket related to your product report ({report.SerialNumber}) has been escalated to the manufacturer by regulators for resolution.",
                    RelatedEntityId = ticketId,
                    RelatedEntityType = "ticket",
                    Priority = NotificationPriority.High
                });
            }

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Sent {Count} notifications for ticket {TicketId} escalation to manufacturer",
                    notifications.Count, ticketId);
            }
        }
    }
}