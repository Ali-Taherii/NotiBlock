using NotiBlock.Backend.Models;
using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class RecallUpdateDTO
    {
        [StringLength(1000, MinimumLength = 10)]
        public string? Reason { get; set; }

        [StringLength(2000, MinimumLength = 10)]
        public string? ActionRequired { get; set; }

        public RecallStatus? Status { get; set; }

        [StringLength(66)]
        public string? TransactionHash { get; set; }
    }
}
