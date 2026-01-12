using System.ComponentModel.DataAnnotations;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.DTOs
{
    public class RegulatorReviewCreateDTO
    {
        [Required]
        public Guid TicketId { get; set; }

        [Required]
        public ReviewDecision Decision { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Notes must be between 10 and 2000 characters")]
        public string Notes { get; set; } = string.Empty;
    }
}