namespace NotiBlock.Backend.Models
{
    public class Consumer
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }

        public ICollection<ConsumerResponse> Responses { get; set; } = new List<ConsumerResponse>();
    }
}