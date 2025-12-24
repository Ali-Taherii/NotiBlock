namespace NotiBlock.Backend.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string SerialNumber { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public Guid ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; } = null!;

        public Guid? ResellerId { get; set; }
        public Reseller? Reseller { get; set; }

        public Guid OwnerId { get; set; }
        public Consumer Owner { get; set; } = null!;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
