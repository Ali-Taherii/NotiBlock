using NotiBlock.Backend.DTOs;

namespace NotiBlock.Backend.Interfaces
{
    public interface IBlockchainService
    {
        // Emit events to blockchain
        Task<RecallBlockchainDTO> EmitRecallIssuedAsync(Guid recallId, string actorRole, string actor);
        Task<RecallBlockchainDTO> EmitRecallStatusChangedAsync(Guid recallId, string newStatus, string actorRole, string actor);

        // Query blockchain
        Task<bool> VerifyRecallOnBlockchainAsync(string transactionHash);
        Task<RecallBlockchainDTO?> GetRecallFromBlockchainAsync(string transactionHash);

        // Metadata
        Task<string> ComputeMetadataHashAsync(object metadata);
        Task<string> PublishMetadataAsync(object metadata);
    }
}
