using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NotiBlock.Backend.Models
{
    public class ResellerTicket
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ResellerId { get; set; }
        public Reseller Reseller { get; set; } = null!;

        [Required]
        public TicketCategory Category { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Pending;

        public int Priority { get; set; } = 0;  // 0=Low, 1=Medium, 2=High, 3=Critical

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Relationships
        public ICollection<ConsumerReport> ConsumerReports { get; set; } = [];

        // Approval/Resolution tracking
        public Guid? ApprovedById { get; set; }
        public Regulator? ApprovedBy { get; set; }

        public Guid? ResolvedBy { get; set; }
        
        [StringLength(2000)]
        public string? ResolutionNotes { get; set; }

        // Soft delete fields
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        // Navigation to RegulatorReviews
        [JsonIgnore]
        public ICollection<RegulatorReview> RegulatorReviews { get; set; } = [];
    }

    public enum TicketCategory
    {
        ProductDefect,
        QualityIssue,
        SafetyConcern,
        CounterfeitSuspicion,
        SupplyChainIssue,
        CustomerComplaint,
        Other
    }

    public enum TicketStatus
    {
        Pending,          // Just created
        UnderReview,      // Regulator is reviewing
        Approved,         // Approved for recall
        Rejected,         // Rejected by regulator
        Resolved,         // Issue resolved
        Closed            // Ticket closed
    }
}
