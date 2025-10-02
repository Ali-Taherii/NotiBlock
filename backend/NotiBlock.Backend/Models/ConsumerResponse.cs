namespace NotiBlock.Backend.Models
{
    public class ConsumerResponse
    {
        public int Id { get; set; }
        public int RecallId { get; set; }
        public Recall Recall { get; set; } = null!;
        public int ConsumerId { get; set; } // FK
        public Consumer Consumer { get; set; } = null!;
        public string ActionTaken { get; set; } = string.Empty;
        public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
    }
}