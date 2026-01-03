using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController(INotificationService service, ILogger<NotificationController> logger) : ControllerBase
    {
        private readonly INotificationService _service = service;
        private readonly ILogger<NotificationController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] bool? isRead,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.GetUserNotificationsAsync(userId, isRead, page, pageSize);
                
                _logger.LogInformation("User {UserId} retrieved their notifications (Page {Page})", userId, page);
                
                return Ok(ApiResponse<PagedResultsDTO<Notification>>.SuccessResponse(
                    result, 
                    $"Retrieved {result.Items.Count} notification(s)"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving notifications"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var notification = await _service.GetNotificationByIdAsync(id, userId);
                
                _logger.LogInformation("Notification {NotificationId} retrieved", id);
                
                return Ok(ApiResponse<Notification>.SuccessResponse(notification));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to notification {NotificationId}", id);
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Notification not found: {NotificationId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification {NotificationId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the notification"));
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var count = await _service.GetUnreadCountAsync(userId);
                
                return Ok(ApiResponse<object>.SuccessResponse(new { unreadCount = count }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while getting unread count"));
            }
        }

        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] NotificationMarkAsReadDTO dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var count = await _service.MarkAsReadAsync(dto.NotificationIds, userId);
                
                _logger.LogInformation("User {UserId} marked {Count} notifications as read", userId, count);
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { markedCount = count }, 
                    $"{count} notification(s) marked as read"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notifications as read");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while marking notifications as read"));
            }
        }

        [HttpPost("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var count = await _service.MarkAllAsReadAsync(userId);
                
                _logger.LogInformation("User {UserId} marked all {Count} notifications as read", userId, count);
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { markedCount = count }, 
                    $"All {count} notification(s) marked as read"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while marking all notifications as read"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _service.DeleteNotificationAsync(id, userId);
                
                _logger.LogInformation("Notification {NotificationId} deleted by user {UserId}", id, userId);
                
                return Ok(ApiResponse.SuccessResponse("Notification deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized delete attempt on notification {NotificationId}", id);
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Notification not found: {NotificationId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting the notification"));
            }
        }
    }
}