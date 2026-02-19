using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class RecallRejectionDTO
    {
        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty;
    }
}
