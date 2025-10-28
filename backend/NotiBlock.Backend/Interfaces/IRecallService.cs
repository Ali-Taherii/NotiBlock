using NotiBlock.Backend.Models;
using NotiBlock.Backend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace NotiBlock.Backend.Interfaces
{
    public interface IRecallService
    {
        Task<Recall> CreateRecallAsync(RecallCreateDTO dto, int manufacturerId);
        Task<Recall> IssueRecallToBlockchainAsync(int recallId);
        Task<Recall> GetRecallByIdAsync(int id);
        Task<Recall> GetRecallByProductIdAsync(string productId);
        Task<IEnumerable<Recall>> GetRecallsByIssueDate(DateTime issuedAt);
        Task<Recall> DeleteRecallByIdAsync(int id);
        Task<Recall> UpdateRecallAsync(int id, RecallCreateDTO dto);
        Task<IEnumerable<Recall>> GetAllRecallsAsync();
    }
}
