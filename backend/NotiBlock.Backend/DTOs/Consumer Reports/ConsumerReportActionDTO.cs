using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class ConsumerReportActionDTO
    {
        [Required(ErrorMessage = "Action is required")]
        public ReportAction Action { get; set; }

        [StringLength(2000, ErrorMessage = "Resolution notes cannot exceed 2000 characters")]
        public string? ResolutionNotes { get; set; }

        // For escalation to reseller ticket
        public Guid? ResellerTicketId { get; set; }
    }

    public enum ReportAction
    {
        Review,      // Reseller starts reviewing
        RequestMoreInfo, // Get more info from the consumer
        Resolve,     // Reseller resolves the issue
        Escalate,    // Escalate to reseller ticket
        Close        // Close the report
    }
}