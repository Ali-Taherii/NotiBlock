using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.DTOs
{
    public class ResellerTicketCreateDTO
    {
        public TicketCategory Category { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;
    }
}
