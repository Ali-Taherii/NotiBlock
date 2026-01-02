using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface IResellerTicketService
    {
        // CRUD operations
        Task<ResellerTicket> CreateTicketAsync(ResellerTicketCreateDTO dto, Guid resellerId);
        Task<ResellerTicket> GetTicketByIdAsync(Guid id);
        Task<ResellerTicket> UpdateTicketAsync(Guid id, ResellerTicketUpdateDTO dto, Guid resellerId);
        Task<bool> DeleteTicketAsync(Guid id, Guid resellerId);

        // List operations
        Task<PagedResultsDTO<ResellerTicket>> GetResellerTicketsAsync(Guid resellerId, int page, int pageSize);
        Task<PagedResultsDTO<ResellerTicket>> GetAllTicketsAsync(int page, int pageSize);
        Task<PagedResultsDTO<ResellerTicket>> GetTicketsByStatusAsync(TicketStatus status, int page, int pageSize);
        Task<PagedResultsDTO<ResellerTicketReadableView>> GetReadableTicketsAsync(Guid? resellerId, int page, int pageSize);

        // Statistics
        Task<object> GetTicketStatisticsAsync(Guid? resellerId = null);

        // Link Consumer Reports to Ticket
        Task<ResellerTicket> LinkConsumerReportsAsync(Guid ticketId, List<Guid> reportIds, Guid resellerId);


    }
}
