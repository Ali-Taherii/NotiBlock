using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services
{
    public class NotificationService(AppDbContext context, ILogger<NotificationService> logger) : INotificationService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<NotificationService> _logger = logger;

        public async Task<Notification> CreateNotificationAsync(NotificationCreateDTO dto)
        {
            var notification = new Notification
            {
                RecipientId = dto.RecipientId,
                RecipientType = dto.RecipientType.ToLowerInvariant(),
                Type = dto.Type,
                Title = dto.Title.Trim(),
                Message = dto.Message.Trim(),
                RelatedEntityId = dto.RelatedEntityId,
                RelatedEntityType = dto.RelatedEntityType?.ToLowerInvariant(),
                Priority = dto.Priority,
                ExpiresAt = dto.ExpiresAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} created for {RecipientType} {RecipientId}: {Title}",
                notification.Id, notification.RecipientType, notification.RecipientId, notification.Title);

            return notification;
        }

        public async Task<Notification> GetNotificationByIdAsync(Guid id, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id) ?? throw new KeyNotFoundException($"Notification with ID {id} not found");

            // Authorization: users can only view their own notifications
            if (notification.RecipientId != userId)
            {
                throw new UnauthorizedAccessException("You can only view your own notifications");
            }

            return notification;
        }

        public async Task<bool> DeleteNotificationAsync(Guid id, Guid userId)
        {
            var notification = await _context.Notifications
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(n => n.Id == id) ?? throw new KeyNotFoundException($"Notification with ID {id} not found");
            if (notification.IsDeleted)
            {
                throw new InvalidOperationException("Notification is already deleted");
            }

            if (notification.RecipientId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own notifications");
            }

            notification.IsDeleted = true;
            notification.DeletedAt = DateTime.UtcNow;
            notification.DeletedBy = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} deleted by user {UserId}",
                id, userId);

            return true;
        }

        public async Task CreateBulkNotificationsAsync(List<NotificationCreateDTO> dtos)
        {
            var notifications = dtos.Select(dto => new Notification
            {
                RecipientId = dto.RecipientId,
                RecipientType = dto.RecipientType.ToLowerInvariant(),
                Type = dto.Type,
                Title = dto.Title.Trim(),
                Message = dto.Message.Trim(),
                RelatedEntityId = dto.RelatedEntityId,
                RelatedEntityType = dto.RelatedEntityType?.ToLowerInvariant(),
                Priority = dto.Priority,
                ExpiresAt = dto.ExpiresAt,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created {Count} bulk notifications", notifications.Count);
        }

        public async Task<int> MarkAsReadAsync(List<Guid> notificationIds, Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => notificationIds.Contains(n.Id) && n.RecipientId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked {Count} notifications as read for user {UserId}",
                notifications.Count, userId);

            return notifications.Count;
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.RecipientId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked all {Count} notifications as read for user {UserId}",
                notifications.Count, userId);

            return notifications.Count;
        }

        public async Task<PagedResultsDTO<Notification>> GetUserNotificationsAsync(Guid userId, bool? isRead, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Notifications
                .Where(n => n.RecipientId == userId);

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            query = query.OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} notifications for user {UserId} (Page {Page})",
                items.Count, userId, page);

            return new PagedResultsDTO<Notification>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == userId && !n.IsRead);
        }

        // ==================== AUTOMATIC TRIGGERS ====================

        public async Task NotifyReportEscalatedAsync(Guid reportId)
        {
            var report = await _context.ConsumerReports
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null || !report.ResellerTicketId.HasValue)
            {
                return;
            }

            // Notify consumer
            await CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = report.ConsumerId,
                RecipientType = "consumer",
                Type = NotificationType.ReportEscalated,
                Title = "Report Escalated",
                Message = $"Your report for product {report.SerialNumber} has been escalated to the reseller for review.",
                RelatedEntityId = reportId,
                RelatedEntityType = "report",
                Priority = NotificationPriority.Normal
            });

            _logger.LogInformation("Notified consumer {ConsumerId} about report {ReportId} escalation",
                report.ConsumerId, reportId);
        }

        public async Task NotifyTicketStatusChangeAsync(Guid ticketId, TicketStatus newStatus)
        {
            var ticket = await _context.ResellerTickets
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
            {
                return;
            }

            var message = newStatus switch
            {
                TicketStatus.Approved => "Your ticket has been approved by the regulator and will be escalated to the manufacturer.",
                TicketStatus.Rejected => "Your ticket has been rejected by the regulator. Please review the feedback.",
                TicketStatus.UnderReview => "Your ticket is now under review by the regulator.",
                TicketStatus.Resolved => "Your ticket has been resolved.",
                _ => $"Your ticket status has been updated to {newStatus}."
            };

            await CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = ticket.ResellerId,
                RecipientType = "reseller",
                Type = newStatus == TicketStatus.Approved ? NotificationType.TicketApproved : NotificationType.TicketRejected,
                Title = $"Ticket Status: {newStatus}",
                Message = message,
                RelatedEntityId = ticketId,
                RelatedEntityType = "ticket",
                Priority = newStatus == TicketStatus.Approved ? NotificationPriority.High : NotificationPriority.Normal
            });

            _logger.LogInformation("Notified reseller {ResellerId} about ticket {TicketId} status change to {Status}",
                ticket.ResellerId, ticketId, newStatus);
        }

        public async Task NotifyProductRegisteredAsync(Guid productId, Guid consumerId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return;
            }

            await CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = consumerId,
                RecipientType = "consumer",
                Type = NotificationType.ProductRegistered,
                Title = "Product Registered Successfully",
                Message = $"Your product ({product.SerialNumber}) has been successfully registered. You will receive notifications about any recalls affecting this product.",
                RelatedEntityId = productId,
                RelatedEntityType = "product",
                Priority = NotificationPriority.Normal
            });

            _logger.LogInformation("Notified consumer {ConsumerId} about product {ProductId} registration",
                consumerId, productId);
        }

        public async Task<int> DeleteExpiredNotificationsAsync()
        {
            var expiredNotifications = await _context.Notifications
                .Where(n => n.ExpiresAt.HasValue && n.ExpiresAt < DateTime.UtcNow && !n.IsDeleted)
                .ToListAsync();

            foreach (var notification in expiredNotifications)
            {
                notification.IsDeleted = true;
                notification.DeletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} expired notifications", expiredNotifications.Count);

            return expiredNotifications.Count;
        }
    }
}