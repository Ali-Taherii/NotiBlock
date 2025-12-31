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

        public RecallStatus Status { get; set; } = RecallStatus.Active;

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        [StringLength(66)]
        public string? TransactionHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft delete fields
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }

    public enum RecallStatus
    {
        Active,
        Resolved,
        Cancelled
    }
}