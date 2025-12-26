namespace NotiBlock.Backend.DTOs
{
    // For reseller registering a product they purchased
    public class ProductRegisterDTO
    {
        public string SerialNumber { get; set; } = string.Empty;
        // ResellerId or ConsumerId comes from JWT claims
    }
}