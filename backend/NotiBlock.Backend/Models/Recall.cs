using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.Models
{
    public class Recall
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ManufacturerId { get; set; }
        public Manufacturer? Manufacturer { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductSerialNumber { get; set; } = string.Empty; // This is the SerialNumber

        // Navigation property to Product using SerialNumber
        public Product? Product { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string ActionRequired { get; set; } = string.Empty;

        public RecallStatus Status { get; set; } = RecallStatus.PendingApproval;

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        [StringLength(66)]
        public string? TransactionHash { get; set; }

        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }

        [StringLength(2000)]
        public string? RegulatorNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft delete fields
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        public ICollection<RecallUpdateRequest> UpdateRequests { get; set; } = new List<RecallUpdateRequest>();
    }

    public enum RecallStatus
    {
        Active = 0,
        Resolved = 1,
        Cancelled = 2,
        PendingApproval = 3,
        Rejected = 4
    }
}