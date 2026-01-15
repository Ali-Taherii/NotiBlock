using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class UpdateRecallStatusDTO
    {
        [Required(ErrorMessage = "New status is required")]
        public string NewStatus { get; set; } = string.Empty;
    }
}
