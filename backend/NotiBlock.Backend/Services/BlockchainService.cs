using Microsoft.Extensions.Options;
using NotiBlock.Backend.Configuration;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.Util;
using Microsoft.EntityFrameworkCore;

namespace NotiBlock.Backend.Services
{
    public class BlockchainService(
        AppDbContext context,
        IOptions<BlockchainSettings> blockchainSettings,
        ILogger<BlockchainService> logger) : IBlockchainService
    {
        private readonly AppDbContext _context = context;
        private readonly BlockchainSettings _settings = blockchainSettings.Value;
        private readonly ILogger<BlockchainService> _logger = logger;

        // Smart contract ABI for RecallRegistry
        private const string RecallRegistryABI = @"[
            {
                ""name"": ""RecallIssued"",
                ""type"": ""event"",
                ""inputs"": [
                    {""name"": ""recallId"", ""type"": ""bytes32"", ""indexed"": true},
                    {""name"": ""productHash"", ""type"": ""bytes32"", ""indexed"": true},
                    {""name"": ""metadataHash"", ""type"": ""string""},
                    {""name"": ""actorRole"", ""type"": ""string""},
                    {""name"": ""actor"", ""type"": ""address""}
                ]
            },
            {
                ""name"": ""RecallStatusChanged"",
                ""type"": ""event"",
                ""inputs"": [
                    {""name"": ""recallId"", ""type"": ""bytes32"", ""indexed"": true},
                    {""name"": ""metadataHash"", ""type"": ""string""},
                    {""name"": ""newStatus"", ""type"": ""string""},
                    {""name"": ""actorRole"", ""type"": ""string""},
                    {""name"": ""actor"", ""type"": ""address""}
                ]
            },
            {
                ""name"": ""issueRecall"",
                ""type"": ""function"",
                ""inputs"": [
                    {""name"": ""recallId"", ""type"": ""bytes32""},
                    {""name"": ""productHash"", ""type"": ""bytes32""},
                    {""name"": ""metadataHash"", ""type"": ""string""},
                    {""name"": ""actorRole"", ""type"": ""string""}
                ],
                ""stateMutability"": ""nonpayable"",
                ""outputs"": [{""name"": """", ""type"": ""bool""}]
            },
            {
                ""name"": ""updateRecallStatus"",
                ""type"": ""function"",
                ""inputs"": [
                    {""name"": ""recallId"", ""type"": ""bytes32""},
                    {""name"": ""newStatus"", ""type"": ""string""},
                    {""name"": ""metadataHash"", ""type"": ""string""}
                ],
                ""stateMutability"": ""nonpayable"",
                ""outputs"": [{""name"": """", ""type"": ""bool""}]
            }
        ]";

        public async Task<RecallBlockchainDTO> EmitRecallIssuedAsync(Guid recallId, string actorRole, string actor)
        {
            try
            {
                var recall = await _context.Recalls.FindAsync(recallId)
                    ?? throw new KeyNotFoundException($"Recall {recallId} not found");

                // Compute metadata hash
                var metadata = new MetadataDTO
                {
                    RecallId = recallId,
                    ProductSerialNumber = recall.ProductSerialNumber,
                    Reason = recall.Reason,
                    ActionRequired = recall.ActionRequired,
                    Status = recall.Status.ToString(),
                    IssuedAt = recall.IssuedAt,
                    ManufacturerId = recall.ManufacturerId.ToString()
                };

                var metadataHash = await ComputeMetadataHashAsync(metadata);
                var productHash = ComputeProductHash(recall.ProductSerialNumber);

                _logger.LogInformation("Emitting RecallIssued event for recall {RecallId}", recallId);

                // Call blockchain
                var web3 = new Web3(new Account(_settings.PrivateKey), _settings.RpcUrl);
                var contractAddress = _settings.ContractAddress;
                var contract = web3.Eth.GetContract(RecallRegistryABI, contractAddress);
                var function = contract.GetFunction("issueRecall");

                var recallIdBytes = recallId.ToByteArray();
                var productHashBytes = Encoding.UTF8.GetBytes(productHash);

                // Send transaction
                var gasEstimate = await function.EstimateGasAsync(recallIdBytes, productHashBytes, metadataHash, actorRole);
                var receipt = await function.SendTransactionAndWaitForReceiptAsync(
                    from: web3.TransactionManager.Account.Address,
                    gas: new Nethereum.Hex.HexTypes.HexBigInteger(gasEstimate),
                    value: null,
                    functionInput: new object[] { recallIdBytes, productHashBytes, metadataHash, actorRole });

                // Store blockchain data
                var blockchainRecall = new BlockchainRecall
                {
                    RecallId = recallId,
                    TransactionHash = receipt.TransactionHash,
                    BlockNumber = (ulong?)receipt.BlockNumber?.Value,
                    ChainId = _settings.ChainId,
                    MetadataHash = metadataHash,
                    EventSignature = "RecallIssued(bytes32,bytes32,string,string,address)",
                    TransactionConfirmedAt = DateTime.UtcNow,
                    ConfirmationCount = 1
                };

                _context.BlockchainRecalls.Add(blockchainRecall);

                // Log event
                var blockchainEvent = new RecallBlockchainEvent
                {
                    RecallId = recallId,
                    EventType = "RecallIssued",
                    TransactionHash = receipt.TransactionHash,
                    BlockNumber = (ulong?)receipt.BlockNumber?.Value,
                    ChainId = _settings.ChainId,
                    MetadataHash = metadataHash,
                    ActorRole = actorRole,
                    Actor = actor
                };

                _context.RecallBlockchainEvents.Add(blockchainEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("RecallIssued event emitted successfully. TxHash: {TxHash}, BlockNumber: {BlockNumber}",
                    receipt.TransactionHash, receipt.BlockNumber?.Value);

                return new RecallBlockchainDTO
                {
                    RecallId = recallId,
                    TransactionHash = receipt.TransactionHash,
                    BlockNumber = (ulong?)receipt.BlockNumber?.Value,
                    ChainId = _settings.ChainId,
                    MetadataHash = metadataHash,
                    EventType = "RecallIssued",
                    TransactionConfirmedAt = DateTime.UtcNow,
                    ConfirmationCount = blockchainRecall.ConfirmationCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error emitting RecallIssued event for recall {RecallId}", recallId);
                throw;
            }
        }

        public async Task<RecallBlockchainDTO> EmitRecallStatusChangedAsync(Guid recallId, string newStatus, string actorRole, string actor)
        {
            try
            {
                var recall = await _context.Recalls.FindAsync(recallId)
                    ?? throw new KeyNotFoundException($"Recall {recallId} not found");

                var blockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.RecallId == recallId)
                    ?? throw new InvalidOperationException($"Recall {recallId} not issued to blockchain yet");

                var metadata = new MetadataDTO
                {
                    RecallId = recallId,
                    ProductSerialNumber = recall.ProductSerialNumber,
                    Reason = recall.Reason,
                    ActionRequired = recall.ActionRequired,
                    Status = newStatus,
                    IssuedAt = recall.IssuedAt,
                    ManufacturerId = recall.ManufacturerId.ToString()
                };

                var metadataHash = await ComputeMetadataHashAsync(metadata);

                _logger.LogInformation("Emitting RecallStatusChanged event for recall {RecallId}, status: {NewStatus}", 
                    recallId, newStatus);

                var web3 = new Web3(new Account(_settings.PrivateKey), _settings.RpcUrl);
                var contractAddress = _settings.ContractAddress;
                var contract = web3.Eth.GetContract(RecallRegistryABI, contractAddress);
                var function = contract.GetFunction("updateRecallStatus");

                var recallIdBytes = recallId.ToByteArray();

                var gasEstimate = await function.EstimateGasAsync(recallIdBytes, newStatus, metadataHash);
                var receipt = await function.SendTransactionAndWaitForReceiptAsync(
                    from: web3.TransactionManager.Account.Address,
                    gas: new Nethereum.Hex.HexTypes.HexBigInteger(gasEstimate),
                    value: null,
                    functionInput: new object[] { recallIdBytes, newStatus, metadataHash });

                // Update blockchain recall record
                blockchainRecall.TransactionHash = receipt.TransactionHash;
                blockchainRecall.BlockNumber = (ulong?)receipt.BlockNumber?.Value;
                blockchainRecall.MetadataHash = metadataHash;
                blockchainRecall.EventSignature = "RecallStatusChanged(bytes32,string,string,string,address)";
                blockchainRecall.LastUpdatedAt = DateTime.UtcNow;

                // Log event
                var blockchainEvent = new RecallBlockchainEvent
                {
                    RecallId = recallId,
                    EventType = "RecallStatusChanged",
                    TransactionHash = receipt.TransactionHash,
                    BlockNumber = (ulong?)receipt.BlockNumber?.Value,
                    ChainId = _settings.ChainId,
                    MetadataHash = metadataHash,
                    ActorRole = actorRole,
                    Actor = actor,
                    NewStatus = newStatus
                };

                _context.RecallBlockchainEvents.Add(blockchainEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("RecallStatusChanged event emitted successfully. TxHash: {TxHash}, BlockNumber: {BlockNumber}",
                    receipt.TransactionHash, receipt.BlockNumber?.Value);

                return new RecallBlockchainDTO
                {
                    RecallId = recallId,
                    TransactionHash = receipt.TransactionHash,
                    BlockNumber = (ulong?)receipt.BlockNumber?.Value,
                    ChainId = _settings.ChainId,
                    MetadataHash = metadataHash,
                    EventType = "RecallStatusChanged",
                    TransactionConfirmedAt = DateTime.UtcNow,
                    ConfirmationCount = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error emitting RecallStatusChanged event for recall {RecallId}", recallId);
                throw;
            }
        }

        public async Task<bool> VerifyRecallOnBlockchainAsync(string transactionHash)
        {
            try
            {
                var web3 = new Web3(_settings.RpcUrl);
                var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                
                var isSuccessful = receipt?.Status?.Value == 1;
                _logger.LogInformation("Recall verification for TxHash {TxHash}: {IsSuccessful}", transactionHash, isSuccessful);
                
                return isSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying recall on blockchain. TxHash: {TxHash}", transactionHash);
                return false;
            }
        }

        public async Task<RecallBlockchainDTO?> GetRecallFromBlockchainAsync(string transactionHash)
        {
            try
            {
                var blockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.TransactionHash == transactionHash);

                if (blockchainRecall == null)
                    return null;

                return new RecallBlockchainDTO
                {
                    RecallId = blockchainRecall.RecallId,
                    TransactionHash = blockchainRecall.TransactionHash,
                    BlockNumber = blockchainRecall.BlockNumber,
                    ChainId = blockchainRecall.ChainId,
                    MetadataHash = blockchainRecall.MetadataHash,
                    EventType = blockchainRecall.EventSignature ?? string.Empty,
                    TransactionConfirmedAt = blockchainRecall.TransactionConfirmedAt,
                    ConfirmationCount = blockchainRecall.ConfirmationCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recall from blockchain. TxHash: {TxHash}", transactionHash);
                return null;
            }
        }

        public async Task<string> ComputeMetadataHashAsync(object metadata)
        {
            try
            {
                var json = JsonSerializer.Serialize(metadata);
                var bytes = Encoding.UTF8.GetBytes(json);
                
                using (var sha256 = SHA256.Create())
                {
                    var hash = await Task.Run(() => sha256.ComputeHash(bytes));
                    var hexHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    return "0x" + hexHash;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing metadata hash");
                throw;
            }
        }

        public async Task<string> PublishMetadataAsync(object metadata)
        {
            // TODO: Implement IPFS publishing
            // For now, return the hash
            return await ComputeMetadataHashAsync(metadata);
        }

        private static string ComputeProductHash(string serialNumber)
        {
            var bytes = Encoding.UTF8.GetBytes(serialNumber);
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(bytes);
                return "0x" + BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
