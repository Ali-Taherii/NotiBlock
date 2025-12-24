namespace NotiBlock.Backend.Models
{
    public class ResellerTicket
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ResellerId { get; set; }
        public Reseller Reseller { get; set; } = null!;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ConsumerReport> ConsumerReports { get; set; } = [];
        
        public int? ApprovedById { get; set; }
        public Regulator? ApprovedBy { get; set; }
    }
}
