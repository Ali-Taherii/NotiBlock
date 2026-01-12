using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.Models
{
    public class RegulatorReview
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TicketId { get; set; }
        public ResellerTicket Ticket { get; set; } = null!;

        [Required]
        public Guid RegulatorId { get; set; }
        public Regulator Regulator { get; set; } = null!;

        [Required]
        public ReviewDecision Decision { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Soft delete fields
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }

    public enum ReviewDecision
    {
        Approved,           // Approved for escalation to manufacturer
        Rejected,           // Rejected - not valid issue
        NeedsMoreInfo,       // Need more information from reseller
        Reopened            // Reopened for further review
    }
}