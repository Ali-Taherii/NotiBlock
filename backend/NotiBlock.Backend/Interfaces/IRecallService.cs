using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface IRecallService
    {
        Task<RecallResponseDTO> CreateRecallAsync(RecallCreateDTO dto, Guid manufacturerId);
        Task<RecallResponseDTO?> GetRecallByIdAsync(Guid id);
        Task<IEnumerable<RecallResponseDTO>> GetAllRecallsAsync(bool includeDeleted = false);
        Task<IEnumerable<RecallResponseDTO>> GetRecallsByManufacturerAsync(Guid manufacturerId);
        Task<IEnumerable<RecallResponseDTO>> GetRecallsByProductAsync(string productId);
        Task<IEnumerable<RecallResponseDTO>> GetPendingRecallsForApprovalAsync();
        Task<RecallResponseDTO?> UpdateRecallAsync(Guid id, RecallUpdateDTO dto, Guid regulatorId);
        Task<bool> SoftDeleteRecallAsync(Guid id, Guid actorId, bool isRegulator);
        Task<bool> ResolveRecallAsync(Guid id, Guid regulatorId);
        Task<RecallResponseDTO> ApproveRecallAsync(Guid recallId, Guid regulatorId, RecallApprovalDTO dto);
        Task<RecallResponseDTO> RejectRecallAsync(Guid recallId, Guid regulatorId, RecallRejectionDTO dto);
        Task<RecallBlockchainDTO> IssueRecallToBlockchainAsync(Guid recallId, Guid regulatorId, string? regulatorNotes = null);
        Task<RecallBlockchainDTO> UpdateRecallStatusOnBlockchainAsync(Guid recallId, string newStatus, Guid regulatorId, string? regulatorNotes = null);
        Task<RecallBlockchainDTO?> GetRecallBlockchainDataAsync(Guid recallId);
        Task<bool> VerifyRecallOnBlockchainAsync(Guid recallId);
        Task<RecallUpdateRequestResponseDTO> CreateUpdateRequestAsync(Guid recallId, RecallUpdateProposalDTO dto, Guid manufacturerId);
        Task<IEnumerable<RecallUpdateRequestResponseDTO>> GetPendingUpdateRequestsAsync();
        Task<RecallResponseDTO> DecideUpdateRequestAsync(Guid requestId, RecallUpdateDecisionDTO dto, Guid regulatorId);
    }
}
