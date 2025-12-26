namespace NotiBlock.Backend.DTOs
{
    public class ProductUpdateDTO
    {
        public required string SerialNumber { get; set; }
        public required string Model { get; set; }
        public Guid? ResellerId { get; set; }
        public Guid? OwnerId { get; set; }
    }
}
