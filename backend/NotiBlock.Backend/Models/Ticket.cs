namespace NotiBlock.Backend.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string IssueDescription { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";

        public int CreatedById { get; set; }
        public AppUser? CreatedBy { get; set; }

        public int? ApprovedById { get; set; }
        public AppUser? ApprovedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
    }
}
