using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class ConsumerReportCreateDTO
    {
        [Required(ErrorMessage = "Serial number is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Serial number must be between 5 and 100 characters")]
        public required string ProductSerialNumber { get; set; }

        [Required(ErrorMessage = "Issue description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public required string IssueDescription { get; set; }

        // Optional photo file (for API multipart form data)
        public IFormFile? PhotoFile { get; set; }
    }
}