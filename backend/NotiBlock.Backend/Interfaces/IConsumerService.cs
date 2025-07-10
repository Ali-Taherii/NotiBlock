using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface IConsumerService
    {
        Task<Consumer> CreateConsumerAsync(ConsumerCreateDTO dto);
        Task<Consumer> GetConsumerByEmailAsync(string email);

        Task<IEnumerable<Consumer>> GetAllConsumersAsync();

        // Task<ConsumerResponse> RespondToRecallAsync(int consumerId, int recallId, string actionTaken);
        // Task<IEnumerable<ConsumerResponse>> GetResponsesByRecallIdAsync(int recallId);
    }
}