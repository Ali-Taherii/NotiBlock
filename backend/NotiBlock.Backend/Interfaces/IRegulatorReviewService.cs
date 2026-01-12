using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface IRegulatorReviewService
    {
        Task<RegulatorReview> CreateReviewAsync(RegulatorReviewCreateDTO dto, Guid regulatorId);
        Task<RegulatorReview> GetReviewByIdAsync(Guid id);
        Task<RegulatorReview> UpdateReviewAsync(Guid id, RegulatorReviewUpdateDTO dto, Guid regulatorId);
        Task<bool> DeleteReviewAsync(Guid id, Guid regulatorId);
        
        // List methods
        Task<PagedResultsDTO<RegulatorReview>> GetReviewsByRegulatorAsync(Guid regulatorId, int page, int pageSize);
        Task<PagedResultsDTO<RegulatorReview>> GetReviewsByTicketAsync(Guid ticketId, int page, int pageSize);
        Task<PagedResultsDTO<ResellerTicket>> GetPendingTicketsAsync(int page, int pageSize);
        
        // Escalation
        Task<ResellerTicket> EscalateToManufacturerAsync(Guid ticketId, Guid regulatorId);
        
        // Statistics
        Task<object> GetRegulatorStatsAsync(Guid regulatorId);
    }
}