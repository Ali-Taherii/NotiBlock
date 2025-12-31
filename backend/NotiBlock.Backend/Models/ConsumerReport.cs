using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.Models
{
    public class ConsumerReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ConsumerId { get; set; }
        public Consumer Consumer { get; set; } = null!;

        // Relationship to Products
        [Required]
        [StringLength(100)]
        public string SerialNumber { get; set; } = string.Empty;
        
        // Navigation property to Product
        public Product? Product { get; set; }   

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Relationship to ResellerTicket
        public Guid? ResellerTicketId { get; set; }
        public ResellerTicket? ResellerTicket { get; set; }

        // Resolution tracking
        public Guid? ResolvedBy { get; set; }  // Reseller who resolved it
        
        [StringLength(2000)]
        public string? ResolutionNotes { get; set; }

        // Soft delete fields
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }

    public enum ReportStatus
    {
        Pending,            // Just submitted by consumer
        UnderReview,        // Being reviewed by reseller
        EscalatedToReseller,// Escalated to reseller ticket
        Resolved,           // Issue resolved
        Closed              // Report closed
    }
}
