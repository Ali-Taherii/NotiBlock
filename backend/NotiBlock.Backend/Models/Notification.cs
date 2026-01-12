using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NotiBlock.Backend.Models
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RecipientId { get; set; }

        [Required]
        [StringLength(50)]
        public string RecipientType { get; set; } = string.Empty; // "consumer", "reseller", "manufacturer", "regulator"

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        // Related entity references (optional)
        public Guid? RelatedEntityId { get; set; }

        [StringLength(50)]
        public string? RelatedEntityType { get; set; } // "product", "recall", "ticket", "report"

        // Notification state
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        // Delivery tracking
        public bool IsEmailSent { get; set; } = false;
        public DateTime? EmailSentAt { get; set; }

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }

    public enum NotificationType
    {
        RecallIssued,           // Recall issued for your product
        RecallAcknowledgement,  // Consumer acknowledged recall
        ReportSubmitted,        // Consumer submitted report
        ReportEscalated,        // Report escalated to ticket
        TicketCreated,          // Reseller created ticket
        TicketApproved,         // Regulator approved ticket
        TicketRejected,         // Regulator rejected ticket
        ReviewSubmitted,        // Regulator submitted review
        ProductRegistered,      // Product registered to consumer
        System,                 // System notifications
        Warning,                // Warning messages
        Info                    // Informational messages
    }

    public enum NotificationPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
}