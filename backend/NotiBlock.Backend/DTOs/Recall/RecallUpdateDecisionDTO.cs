using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class RecallUpdateDecisionDTO
    {
        [Required]
        public bool Approve { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }
    }
}
