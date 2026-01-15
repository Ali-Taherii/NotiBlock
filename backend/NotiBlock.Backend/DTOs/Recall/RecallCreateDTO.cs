using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class RecallCreateDTO
    {
        // Manufacturer ID comes from JWT claims
        [Required]
        [StringLength(100)]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string ActionRequired { get; set; } = string.Empty;
    }
}