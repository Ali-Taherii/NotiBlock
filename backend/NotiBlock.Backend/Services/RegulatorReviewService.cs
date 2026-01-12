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

            // Notify reseller of ticket status change
            await _notificationService.NotifyTicketStatusChangeAsync(dto.TicketId, ticket.Status);

            _logger.LogInformation("Review {ReviewId} created by regulator {RegulatorId} for ticket {TicketId} with decision {Decision}",
                review.Id, regulatorId, dto.TicketId, dto.Decision);

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
            ticket.Status = TicketStatus.UnderReview; // Or create a new status like "EscalatedToManufacturer"
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketId} escalated to manufacturer by regulator {RegulatorId}",
                ticketId, regulatorId);

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
    }
}