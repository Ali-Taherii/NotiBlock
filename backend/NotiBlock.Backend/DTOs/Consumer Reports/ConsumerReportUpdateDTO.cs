using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class ConsumerReportUpdateDTO
    {
        [Required(ErrorMessage = "Issue description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string IssueDescription { get; set; } = string.Empty;
    }
}