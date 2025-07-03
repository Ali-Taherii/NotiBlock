namespace NotiBlock.Backend.Models
{
    public class Recall
    {
        public int Id { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}
