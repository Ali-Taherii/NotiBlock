using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public Manufacturer? Manufacturer { get; set; }

        public Guid? ResellerId { get; set; }
        [JsonIgnore]
        public Reseller? Reseller { get; set; }

        public Guid? OwnerId { get; set; }
        [JsonIgnore]
        public Consumer? Owner { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Soft delete fields
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
