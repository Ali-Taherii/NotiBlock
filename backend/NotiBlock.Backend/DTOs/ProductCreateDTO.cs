namespace NotiBlock.Backend.DTOs
{
    // For manufacturer creating a product
    public class ProductCreateDTO
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        // ManufacturerId comes from JWT claims, not the request body 
    }
}
