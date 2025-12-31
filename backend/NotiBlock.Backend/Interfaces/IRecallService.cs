using NotiBlock.Backend.DTOs;

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
    }
}
