using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface INotificationService
    {
        // Core CRUD
        Task<Notification> CreateNotificationAsync(NotificationCreateDTO dto);
        Task<Notification> GetNotificationByIdAsync(Guid id, Guid userId);
        Task<bool> DeleteNotificationAsync(Guid id, Guid userId);

        // Bulk operations
        Task CreateBulkNotificationsAsync(List<NotificationCreateDTO> dtos);
        Task<int> MarkAsReadAsync(List<Guid> notificationIds, Guid userId);
        Task<int> MarkAllAsReadAsync(Guid userId);

        // List methods
        Task<PagedResultsDTO<Notification>> GetUserNotificationsAsync(Guid userId, bool? isRead, int page, int pageSize);
        Task<int> GetUnreadCountAsync(Guid userId);

        // Automatic notification triggers
        Task NotifyRecallIssuedAsync(Guid recallId);
        Task NotifyReportEscalatedAsync(Guid reportId);
        Task NotifyTicketStatusChangeAsync(Guid ticketId, TicketStatus newStatus);
        Task NotifyProductRegisteredAsync(Guid productId, Guid consumerId);

        // Cleanup
        Task<int> DeleteExpiredNotificationsAsync();
    }
}