using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.Models
{
    public class RecallUpdateRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RecallId { get; set; }
        public Recall? Recall { get; set; }

        [Required]
        public Guid ManufacturerId { get; set; }
        public Manufacturer? Manufacturer { get; set; }

        [StringLength(1000, MinimumLength = 10)]
        public string? ProposedReason { get; set; }

        [StringLength(2000, MinimumLength = 10)]
        public string? ProposedActionRequired { get; set; }

        public RecallStatus? ProposedStatus { get; set; }

        [StringLength(2000)]
        public string? ManufacturerNotes { get; set; }

        public RecallUpdateRequestStatus Status { get; set; } = RecallUpdateRequestStatus.Pending;

        public Guid? ReviewedBy { get; set; }
        public Regulator? ReviewedByRegulator { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(2000)]
        public string? RegulatorNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }

    public enum RecallUpdateRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
