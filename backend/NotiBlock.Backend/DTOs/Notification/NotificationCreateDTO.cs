using System.ComponentModel.DataAnnotations;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.DTOs
{
    public class NotificationCreateDTO
    {
        [Required]
        public Guid RecipientId { get; set; }

        [Required]
        public string RecipientType { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public DateTime? ExpiresAt { get; set; }
    }
}