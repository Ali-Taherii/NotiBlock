using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class LinkReportsToTicketDTO
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one report ID is required")]
        public List<Guid> ReportIds { get; set; } = [];
    }
}