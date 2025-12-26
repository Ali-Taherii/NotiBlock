using System.ComponentModel.DataAnnotations;

    namespace NotiBlock.Backend.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Model { get; set; } = string.Empty;

        [Required]
        public Guid ManufacturerId { get; set; }

        public Guid? ResellerId { get; set; }

        public Guid? OwnerId { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Soft delete fields
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        // Navigation properties (optional - depends on biz logic)
        // public Manufacturer? Manufacturer { get; set; }
        // public Reseller? Reseller { get; set; }
        // public Consumer? Owner { get; set; }
    }
}
