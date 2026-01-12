using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.DTOs
{
    public class RegulatorReviewResponseDTO
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Guid RegulatorId { get; set; }
        public ReviewDecision Decision { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Include minimal ticket info (no circular refs)
        public TicketSummaryDTO? Ticket { get; set; }
        public RegulatorSummaryDTO? Regulator { get; set; }
    }

    public class TicketSummaryDTO
    {
        public Guid Id { get; set; }
        public TicketCategory Category { get; set; }
        public TicketStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Include reseller info without circular refs
        public string ResellerCompanyName { get; set; } = string.Empty;
        public int ConsumerReportsCount { get; set; }
    }

    public class RegulatorSummaryDTO
    {
        public Guid Id { get; set; }
        public string AgencyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}