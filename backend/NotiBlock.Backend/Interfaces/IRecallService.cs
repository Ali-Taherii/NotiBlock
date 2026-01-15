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
        Task<RecallResponseDTO?> UpdateRecallAsync(Guid id, RecallUpdateDTO dto, Guid manufacturerId);
        Task<bool> SoftDeleteRecallAsync(Guid id, Guid manufacturerId);
        Task<bool> ResolveRecallAsync(Guid id, Guid manufacturerId);
        Task<RecallBlockchainDTO> IssueRecallToBlockchainAsync(Guid recallId, Guid manufacturerId);
        Task<RecallBlockchainDTO> UpdateRecallStatusOnBlockchainAsync(Guid recallId, string newStatus, Guid manufacturerId);
        Task<RecallBlockchainDTO?> GetRecallBlockchainDataAsync(Guid recallId);
        Task<bool> VerifyRecallOnBlockchainAsync(Guid recallId);
    }
}
