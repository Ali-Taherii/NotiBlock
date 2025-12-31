using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class ResellerTicketActionDTO
    {
        [Required(ErrorMessage = "Action is required")]
        public TicketAction Action { get; set; }

        [StringLength(2000, ErrorMessage = "Resolution notes cannot exceed 2000 characters")]
        public string? ResolutionNotes { get; set; }
    }

    public enum TicketAction
    {
        Approve = 0,
        Reject = 1,
        Resolve = 2,
        Close = 3,
        Reopen = 4
    }
}
