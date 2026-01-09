namespace NotiBlock.Backend.DTOs
{
    public class ProductResponseDTO
    {
        public Guid Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        
        // Manufacturer details
        public Guid ManufacturerId { get; set; }
        public ManufacturerBasicDTO? Manufacturer { get; set; }
        
        // Reseller details (optional)
        public Guid? ResellerId { get; set; }
        public ResellerBasicDTO? Reseller { get; set; }
        
        // Owner details (optional)
        public Guid? OwnerId { get; set; }
        public ConsumerBasicDTO? Owner { get; set; }
    }

    // Basic DTOs to avoid circular references and expose only necessary data
    public class ManufacturerBasicDTO
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string WalletAddress { get; set; } = string.Empty;
    }

    public class ResellerBasicDTO
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string WalletAddress { get; set; } = string.Empty;
    }

    public class ConsumerBasicDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string WalletAddress { get; set; } = string.Empty;
    }
}
