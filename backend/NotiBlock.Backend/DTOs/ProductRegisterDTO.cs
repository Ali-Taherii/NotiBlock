namespace NotiBlock.Backend.DTOs
{
    public class ProductRegisterDTO
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public Guid ManufacturerId { get; set; }
        public Guid? ResellerId { get; set; }
    }
}
