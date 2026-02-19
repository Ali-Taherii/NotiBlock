using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class RecallApprovalDTO
    {
        [StringLength(2000)]
        public string? Notes { get; set; }
    }
}
