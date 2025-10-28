using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class RecallCreateDTO
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public string ActionRequired { get; set; } = string.Empty;
    }
}