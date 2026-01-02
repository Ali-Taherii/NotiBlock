using System.ComponentModel.DataAnnotations;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.DTOs
{
    public class RegulatorReviewUpdateDTO
    {
        [Required]
        public ReviewDecision Decision { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Notes { get; set; } = string.Empty;
    }
}