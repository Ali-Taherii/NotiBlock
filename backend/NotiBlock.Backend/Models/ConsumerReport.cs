namespace NotiBlock.Backend.Models
{
    public class ConsumerReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ConsumerId { get; set; }
        public Consumer Consumer { get; set; } = null!;

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? ResellerTicketId { get; set; }
        public ResellerTicket? ResellerTicket { get; set; }
    }
}
