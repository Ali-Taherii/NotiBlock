using NotiBlock.Backend.Models;
using NotiBlock.Backend.DTOs;

namespace NotiBlock.Backend.Interfaces
{
    public interface ITicketService
    {
        Task<Ticket> CreateTicketAsync(TicketCreateDTO dto, int userId);
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();
        Task<Ticket?> ApproveTicketAsync(int ticketId, int regulatorId);
        Task<IEnumerable<Ticket>> GetTicketsByUserId(int userId);
        Task<Ticket?> UpdateTicketAsync(int ticketId, TicketCreateDTO dto, int userId);
        Task<Ticket?> DeleteTicketAsync(int ticketId, int userId);

    }
}
