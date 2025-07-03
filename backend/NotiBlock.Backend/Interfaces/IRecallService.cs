using NotiBlock.Backend.Models;
using NotiBlock.Backend.DTOs;

namespace NotiBlock.Backend.Interfaces
{
    public interface IRecallService
    {
        Task<Recall> CreateRecallAsync(RecallCreateDto dto);
        Task<IEnumerable<Recall>> GetAllRecallsAsync();
    }
}
