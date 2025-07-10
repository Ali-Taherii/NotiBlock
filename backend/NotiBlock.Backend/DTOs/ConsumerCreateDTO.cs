namespace NotiBlock.Backend.DTOs
{
    public class ConsumerCreateDTO
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
    }
}