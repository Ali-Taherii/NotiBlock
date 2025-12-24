using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface IResellerTicketService
    {
        Task<ResellerTicket> CreateTicketAsync(ResellerTicketDTO dto, Guid resellerId);
    }
}
