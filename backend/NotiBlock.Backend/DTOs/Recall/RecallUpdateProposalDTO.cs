using NotiBlock.Backend.Models;
using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class RecallUpdateProposalDTO
    {
        [StringLength(1000, MinimumLength = 10)]
        public string? Reason { get; set; }

        [StringLength(2000, MinimumLength = 10)]
        public string? ActionRequired { get; set; }

        public RecallStatus? Status { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }
    }
}
