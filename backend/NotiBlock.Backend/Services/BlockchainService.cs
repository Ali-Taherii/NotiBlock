using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using NotiBlock.Backend.Configuration;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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

        // Default gas limits for transactions
        private const ulong DefaultGasLimit = 500000;
        private const ulong MinGasLimit = 21000;

        // Confirmation thresholds
        private const int MinConfirmationsForFinality = 12;

        // Smart contract ABI for RecallRegistry - CORRECTED to match actual contract
        private const string RecallRegistryABI = @"[
            {
                ""name"": ""RecallIssued"",
                ""type"": ""event"",
                ""anonymous"": false,
                ""inputs"": [
                    {""name"": ""recallId"", ""type"": ""uint256"", ""indexed"": true},
                    {""name"": ""productHash"", ""type"": ""bytes32"", ""indexed"": true},
                    {""name"": ""metadataHash"", ""type"": ""bytes32"", ""indexed"": true},
                    {""name"": ""actorRole"", ""type"": ""string"", ""indexed"": false},
                    {""name"": ""actor"", ""type"": ""address"", ""indexed"": false}
                ]
            },
            {
                ""name"": ""RecallStatusChanged"",
                ""type"": ""event"",
                ""anonymous"": false,
                ""inputs"": [
                    {""name"": ""recallId"", ""type"": ""uint256"", ""indexed"": true},
                    {""name"": ""metadataHash"", ""type"": ""bytes32"", ""indexed"": true},
                    {""name"": ""newStatus"", ""type"": ""string"", ""indexed"": false},
                    {""name"": ""actorRole"", ""type"": ""string"", ""indexed"": false},
                    {""name"": ""actor"", ""type"": ""address"", ""indexed"": false}
                ]
            },
            {
                ""name"": ""emitRecallIssued"",
                ""type"": ""function"",
                ""inputs"": [
                    {""name"": ""recallId"", ""type"": ""uint256""},
                    {""name"": ""productHash"", ""type"": ""bytes32""},
                    {""name"": ""metadataHash"", ""type"": ""bytes32""},
                    {""name"": ""actorRole"", ""type"": ""string""}
                ],
                ""stateMutability"": ""nonpayable"",
                ""outputs"": []
            },
            {
                ""name"": ""emitRecallStatusChanged"",
                ""type"": ""function"",
                ""inputs"": [
                    {""name"": ""recallId"", ""type"": ""uint256""},
                    {""name"": ""metadataHash"", ""type"": ""bytes32""},
                    {""name"": ""newStatus"", ""type"": ""string""},
                    {""name"": ""actorRole"", ""type"": ""string""}
                ],
                ""stateMutability"": ""nonpayable"",
                ""outputs"": []
            }
        ]";

        public async Task<RecallBlockchainDTO> EmitRecallIssuedAsync(Guid recallId, string actorRole, string actor)
        {
            _logger.LogInformation("Starting EmitRecallIssuedAsync for recall {RecallId}", recallId);

            // Track entities for potential rollback
            BlockchainRecall? blockchainRecall = null;
            RecallBlockchainEvent? blockchainEvent = null;

            try
            {
                // Validate inputs
                ValidateInputs(actorRole, actor);

                // Fetch recall from database
                var recall = await _context.Recalls.FindAsync(recallId)
                    ?? throw new KeyNotFoundException($"Recall {recallId} not found");

                // Prepare metadata
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

                // Compute hashes
                var metadataHashBytes = await ComputeMetadataHashBytesAsync(metadata);
                var productHashBytes = ComputeProductHashBytes(recall.ProductSerialNumber);

                _logger.LogInformation(
                    "Computed hashes - MetadataHash: {MetadataHash}, ProductHash: {ProductHash}",
                    metadataHashBytes.ToHex(), productHashBytes.ToHex());

                // Convert GUID to uint256 (BigInteger) - use the first 16 bytes as a number
                var recallIdBytes = recallId.ToByteArray();
                var recallIdUint256 = new BigInteger(recallIdBytes, isUnsigned: true);

                _logger.LogInformation(
                    "Transaction parameters - RecallId (uint256): {RecallId}, ProductHash: {ProductHash}, MetadataHash: {MetadataHash}, ActorRole: {ActorRole}",
                    recallIdUint256, productHashBytes.ToHex(), metadataHashBytes.ToHex(), actorRole);

                var receipt = await SendTransactionAsync(
                    functionName: "emitRecallIssued",
                    parameters: new object[] { recallIdUint256, productHashBytes, metadataHashBytes, actorRole });

                if (receipt == null)
                {
                    throw new InvalidOperationException("Transaction receipt is null - transaction may have failed");
                }

                // Verify event was emitted
                var eventVerified = await VerifyEventEmittedAsync(receipt, "RecallIssued");
                if (!eventVerified)
                {
                    _logger.LogWarning("RecallIssued event was not found in transaction logs. Transaction may not have executed as expected.");
                }

                _logger.LogInformation("Transaction completed successfully. TxHash: {TxHash}", receipt.TransactionHash);

                // Get initial confirmation count
                var confirmationCount = await GetConfirmationCountAsync(receipt.TransactionHash);

                // Store blockchain data
                blockchainRecall = new BlockchainRecall
                {
                    RecallId = recallId,
                    TransactionHash = receipt.TransactionHash,
                    BlockNumber = (ulong?)(receipt.BlockNumber?.Value),
                    ChainId = _settings.ChainId,
                    MetadataHash = "0x" + metadataHashBytes.ToHex(),
                    EventSignature = "RecallIssued(uint256,bytes32,bytes32,string,address)",
                    TransactionConfirmedAt = DateTime.UtcNow,
                    ConfirmationCount = confirmationCount
                };

                _context.BlockchainRecalls.Add(blockchainRecall);

                // Log blockchain event
                blockchainEvent = new RecallBlockchainEvent
                {
                    RecallId = recallId,
                    EventType = "RecallIssued",
                    TransactionHash = receipt.TransactionHash,
                    BlockNumber = (ulong?)(receipt.BlockNumber?.Value),
                    ChainId = _settings.ChainId,
                    MetadataHash = "0x" + metadataHashBytes.ToHex(),
                    ActorRole = actorRole,
                    Actor = actor
                };

                _context.RecallBlockchainEvents.Add(blockchainEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "RecallIssued event emitted successfully. RecallId: {RecallId}, TxHash: {TxHash}, BlockNumber: {BlockNumber}, Confirmations: {Confirmations}",
                    recallId, receipt.TransactionHash, receipt.BlockNumber?.Value, confirmationCount);

                return new RecallBlockchainDTO
                {
                    RecallId = recallId,
                    TransactionHash = receipt.TransactionHash,
                    BlockNumber = (ulong?)(receipt.BlockNumber?.Value),
                    ChainId = _settings.ChainId,
                    MetadataHash = "0x" + metadataHashBytes.ToHex(),
                    EventType = "RecallIssued",
                    TransactionConfirmedAt = DateTime.UtcNow,
                    ConfirmationCount = confirmationCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error emitting RecallIssued event for recall {RecallId}. Initiating rollback.", recallId);

                // Rollback database changes if they were made
                await RollbackDatabaseChangesAsync(blockchainRecall, blockchainEvent);

                throw;
            }
        }

        public async Task<RecallBlockchainDTO> EmitRecallStatusChangedAsync(Guid recallId, string newStatus, string actorRole, string actor)
        {
            _logger.LogInformation("Starting EmitRecallStatusChangedAsync for recall {RecallId} with status {NewStatus}", recallId, newStatus);

            RecallBlockchainEvent? blockchainEvent = null;
            BlockchainRecall? originalBlockchainRecall = null;

            try
            {
                // Validate inputs
                ValidateInputs(actorRole, actor, newStatus);

                // Fetch recall from database
                var recall = await _context.Recalls.FindAsync(recallId)
                    ?? throw new KeyNotFoundException($"Recall {recallId} not found");

                // Verify recall was already issued to blockchain
                var blockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.RecallId == recallId)
                    ?? throw new InvalidOperationException($"Recall {recallId} not issued to blockchain yet");

                // Store original state for rollback
                originalBlockchainRecall = new BlockchainRecall
                {
                    RecallId = blockchainRecall.RecallId,
                    TransactionHash = blockchainRecall.TransactionHash,
                    BlockNumber = blockchainRecall.BlockNumber,
                    ChainId = blockchainRecall.ChainId,
                    MetadataHash = blockchainRecall.MetadataHash,
                    EventSignature = blockchainRecall.EventSignature,
                    TransactionConfirmedAt = blockchainRecall.TransactionConfirmedAt,
                    ConfirmationCount = blockchainRecall.ConfirmationCount,
                    LastUpdatedAt = blockchainRecall.LastUpdatedAt
                };

                // Prepare metadata with new status
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

                // Compute hash
                var metadataHashBytes = await ComputeMetadataHashBytesAsync(metadata);

                _logger.LogInformation("Computed new MetadataHash: {MetadataHash}", metadataHashBytes.ToHex());

                // Convert GUID to uint256 (BigInteger)
                var recallIdBytes = recallId.ToByteArray();
                var recallIdUint256 = new BigInteger(recallIdBytes, isUnsigned: true);

                var receipt = await SendTransactionAsync(
                    functionName: "emitRecallStatusChanged",
                    parameters: new object[] { recallIdUint256, metadataHashBytes, newStatus, actorRole });

                if (receipt == null)
                {
                    throw new InvalidOperationException("Transaction receipt is null - transaction may have failed");
                }

                // Verify event was emitted
                var eventVerified = await VerifyEventEmittedAsync(receipt, "RecallStatusChanged");
                if (!eventVerified)
                {
                    _logger.LogWarning("RecallStatusChanged event was not found in transaction logs. Transaction may not have executed as expected.");
                }

                // Get confirmation count
                var confirmationCount = await GetConfirmationCountAsync(receipt.TransactionHash);

                // Update blockchain recall record
                blockchainRecall.TransactionHash = receipt.TransactionHash;
                blockchainRecall.BlockNumber = (ulong?)(receipt.BlockNumber?.Value);
                blockchainRecall.MetadataHash = "0x" + metadataHashBytes.ToHex();
                blockchainRecall.EventSignature = "RecallStatusChanged(uint256,bytes32,string,string,address)";
                blockchainRecall.LastUpdatedAt = DateTime.UtcNow;
                blockchainRecall.ConfirmationCount = confirmationCount;

                // Log blockchain event
                blockchainEvent = new RecallBlockchainEvent
                {
                    RecallId = recallId,
                    EventType = "RecallStatusChanged",
                    TransactionHash = receipt.TransactionHash,
                    BlockNumber = (ulong?)(receipt.BlockNumber?.Value),
                    ChainId = _settings.ChainId,
                    MetadataHash = "0x" + metadataHashBytes.ToHex(),
                    ActorRole = actorRole,
                    Actor = actor,
                    NewStatus = newStatus
                };

                _context.RecallBlockchainEvents.Add(blockchainEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "RecallStatusChanged event emitted successfully. RecallId: {RecallId}, TxHash: {TxHash}, BlockNumber: {BlockNumber}, Confirmations: {Confirmations}",
                    recallId, receipt.TransactionHash, receipt.BlockNumber?.Value, confirmationCount);

                return new RecallBlockchainDTO
                {
                    RecallId = recallId,
                    TransactionHash = receipt.TransactionHash,
                    BlockNumber = (ulong?)(receipt.BlockNumber?.Value),
                    ChainId = _settings.ChainId,
                    MetadataHash = "0x" + metadataHashBytes.ToHex(),
                    EventType = "RecallStatusChanged",
                    TransactionConfirmedAt = DateTime.UtcNow,
                    ConfirmationCount = confirmationCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error emitting RecallStatusChanged event for recall {RecallId}. Initiating rollback.", recallId);

                // Rollback database changes
                await RollbackStatusUpdateAsync(recallId, originalBlockchainRecall, blockchainEvent);

                throw;
            }
        }

        public async Task<bool> VerifyRecallOnBlockchainAsync(string transactionHash)
        {
            _logger.LogInformation("Verifying recall on blockchain. TxHash: {TxHash}", transactionHash);

            try
            {
                if (string.IsNullOrWhiteSpace(transactionHash))
                {
                    _logger.LogWarning("Transaction hash is null or empty");
                    return false;
                }

                var web3 = new Web3(_settings.RpcUrl);
                var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

                if (receipt == null)
                {
                    _logger.LogWarning("Transaction receipt not found for TxHash: {TxHash}", transactionHash);
                    return false;
                }

                var isSuccessful = receipt.Status?.Value == 1;
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
            _logger.LogInformation("Retrieving recall from blockchain. TxHash: {TxHash}", transactionHash);

            try
            {
                if (string.IsNullOrWhiteSpace(transactionHash))
                {
                    _logger.LogWarning("Transaction hash is null or empty");
                    return null;
                }

                var blockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.TransactionHash == transactionHash);

                if (blockchainRecall == null)
                {
                    _logger.LogWarning("Blockchain recall not found for TxHash: {TxHash}", transactionHash);
                    return null;
                }

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
            var hashBytes = await ComputeMetadataHashBytesAsync(metadata);
            return "0x" + hashBytes.ToHex();
        }

        public async Task<string> PublishMetadataAsync(object metadata)
        {
            return await ComputeMetadataHashAsync(metadata);
        }

        /// <summary>
        /// Updates confirmation count for a blockchain transaction
        /// </summary>
        public async Task<int> UpdateConfirmationCountAsync(string transactionHash)
        {
            try
            {
                var blockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.TransactionHash == transactionHash);

                if (blockchainRecall == null)
                {
                    _logger.LogWarning("Blockchain recall not found for TxHash: {TxHash}", transactionHash);
                    return 0;
                }

                var confirmationCount = await GetConfirmationCountAsync(transactionHash);

                blockchainRecall.ConfirmationCount = confirmationCount;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated confirmation count for TxHash {TxHash}: {ConfirmationCount} confirmations",
                    transactionHash, confirmationCount);

                return confirmationCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating confirmation count for TxHash: {TxHash}", transactionHash);
                return 0;
            }
        }

        /// <summary>
        /// Checks if a transaction has reached finality (sufficient confirmations)
        /// </summary>
        public async Task<bool> IsTransactionFinalAsync(string transactionHash)
        {
            try
            {
                var confirmationCount = await GetConfirmationCountAsync(transactionHash);
                var isFinal = confirmationCount >= MinConfirmationsForFinality;

                _logger.LogInformation(
                    "Transaction {TxHash} finality check: {Confirmations}/{RequiredConfirmations} confirmations, Final: {IsFinal}",
                    transactionHash, confirmationCount, MinConfirmationsForFinality, isFinal);

                return isFinal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking transaction finality for TxHash: {TxHash}", transactionHash);
                return false;
            }
        }

        // ===== PRIVATE HELPER METHODS =====

        /// <summary>
        /// Computes the SHA256 hash of metadata and returns raw bytes
        /// </summary>
        private async Task<byte[]> ComputeMetadataHashBytesAsync(object metadata)
        {
            try
            {
                var json = JsonSerializer.Serialize(metadata);
                var bytes = Encoding.UTF8.GetBytes(json);

                using (var sha256 = SHA256.Create())
                {
                    var hash = await Task.Run(() => sha256.ComputeHash(bytes));
                    return hash;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing metadata hash");
                throw;
            }
        }

        /// <summary>
        /// Computes the SHA256 hash of a serial number as raw bytes
        /// </summary>
        private static byte[] ComputeProductHashBytes(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new ArgumentException("Serial number cannot be null or empty");
            }

            var bytes = Encoding.UTF8.GetBytes(serialNumber);
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(bytes);
            }
        }

        /// <summary>
        /// Gets the current confirmation count for a transaction
        /// </summary>
        private async Task<int> GetConfirmationCountAsync(string transactionHash)
        {
            try
            {
                var web3 = new Web3(_settings.RpcUrl);
                var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

                if (receipt?.BlockNumber == null)
                {
                    _logger.LogWarning("Cannot get confirmation count - receipt or block number is null for TxHash: {TxHash}", transactionHash);
                    return 0;
                }

                var currentBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                var confirmations = (int)(currentBlock.Value - receipt.BlockNumber.Value);

                return Math.Max(confirmations, 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting confirmation count for TxHash: {TxHash}", transactionHash);
                return 1; // Return 1 as minimum if we can't determine
            }
        }

        /// <summary>
        /// Verifies that the expected event was emitted in the transaction
        /// </summary>
        private async Task<bool> VerifyEventEmittedAsync(
            Nethereum.RPC.Eth.DTOs.TransactionReceipt receipt,
            string eventName)
        {
            try
            {
                var web3 = new Web3(_settings.RpcUrl);
                var contract = web3.Eth.GetContract(RecallRegistryABI, _settings.ContractAddress);

                // Get the event from ABI
                var contractEvent = contract.GetEvent(eventName);

                if (contractEvent == null)
                {
                    _logger.LogWarning("Event {EventName} not found in contract ABI", eventName);
                    return false;
                }

                // Compute the event signature hash (topic[0]) manually
                var eventSignature = contractEvent.EventABI.Sha3Signature;

                var logs = receipt.Logs;

                if (logs == null || logs.Length == 0)
                {
                    _logger.LogWarning("No logs found in transaction receipt for event {EventName}", eventName);
                    return false;
                }

                foreach (var log in logs)
                {
                    if (log.Topics != null && log.Topics.Length > 0)
                    {
                        if (log.Topics[0].ToString().Equals(eventSignature, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation(
                                "Event {EventName} verified in transaction {TxHash}. Event signature hash: {SignatureHash}",
                                eventName, receipt.TransactionHash, eventSignature);
                            return true;
                        }
                    }
                }

                _logger.LogWarning(
                    "Event {EventName} NOT found in transaction logs for {TxHash}. Expected signature: {SignatureHash}",
                    eventName, receipt.TransactionHash, eventSignature);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying event emission for event {EventName}", eventName);
                return false;
            }
        }

        /// <summary>
        /// Rolls back database changes for failed recall issuance
        /// </summary>
        private async Task RollbackDatabaseChangesAsync(
            BlockchainRecall? blockchainRecall,
            RecallBlockchainEvent? blockchainEvent)
        {
            try
            {
                if (blockchainRecall != null && _context.Entry(blockchainRecall).State != EntityState.Detached)
                {
                    _context.BlockchainRecalls.Remove(blockchainRecall);
                    _logger.LogWarning("Rolled back BlockchainRecall for RecallId: {RecallId}", blockchainRecall.RecallId);
                }

                if (blockchainEvent != null && _context.Entry(blockchainEvent).State != EntityState.Detached)
                {
                    _context.RecallBlockchainEvents.Remove(blockchainEvent);
                    _logger.LogWarning("Rolled back RecallBlockchainEvent for RecallId: {RecallId}", blockchainEvent.RecallId);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Database rollback completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database rollback. Manual intervention may be required.");
            }
        }

        /// <summary>
        /// Rolls back status update changes
        /// </summary>
        private async Task RollbackStatusUpdateAsync(
            Guid recallId,
            BlockchainRecall? originalState,
            RecallBlockchainEvent? newEvent)
        {
            try
            {
                var blockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.RecallId == recallId);

                if (blockchainRecall != null && originalState != null)
                {
                    // Restore original state
                    blockchainRecall.TransactionHash = originalState.TransactionHash;
                    blockchainRecall.BlockNumber = originalState.BlockNumber;
                    blockchainRecall.MetadataHash = originalState.MetadataHash;
                    blockchainRecall.EventSignature = originalState.EventSignature;
                    blockchainRecall.ConfirmationCount = originalState.ConfirmationCount;
                    blockchainRecall.LastUpdatedAt = originalState.LastUpdatedAt;

                    _logger.LogWarning("Restored original BlockchainRecall state for RecallId: {RecallId}", recallId);
                }

                if (newEvent != null && _context.Entry(newEvent).State != EntityState.Detached)
                {
                    _context.RecallBlockchainEvents.Remove(newEvent);
                    _logger.LogWarning("Removed failed RecallBlockchainEvent for RecallId: {RecallId}", recallId);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Status update rollback completed successfully for RecallId: {RecallId}", recallId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during status update rollback for RecallId: {RecallId}. Manual intervention may be required.", recallId);
            }
        }

        /// <summary>
        /// Estimates gas limit with comprehensive error handling and fallback logic
        /// </summary>
        private Task<ulong> EstimateGasLimitAsync(Nethereum.Contracts.Function function, object[] parameters)
        {
            // Skip gas estimation entirely due to Nethereum bug with null RPC error data
            // This occurs when contract would revert, causing NullReferenceException in Nethereum's error handler
            _logger.LogInformation("Using default gas limit of {DefaultGasLimit} (gas estimation skipped)", DefaultGasLimit);
            return Task.FromResult(DefaultGasLimit);
        }

        /// <summary>
        /// Sends a transaction to the blockchain and waits for receipt
        /// </summary>
        private async Task<Nethereum.RPC.Eth.DTOs.TransactionReceipt?> SendTransactionAsync(
            string functionName,
            object[] parameters)
        {
            try
            {
                _logger.LogInformation("Preparing blockchain transaction for function: {FunctionName}", functionName);
                _logger.LogInformation("Parameters count: {ParameterCount}", parameters.Length);

                // Validate configuration
                ValidateBlockchainSettings();

                // Create account and Web3 instance
                var formattedPrivateKey = FormatPrivateKey(_settings.PrivateKey);
                var account = new Account(formattedPrivateKey, _settings.ChainId);
                var web3 = new Web3(account, _settings.RpcUrl);

                _logger.LogInformation("Web3 instance created. Account: {Account}, RPC: {RpcUrl}",
                    account.Address, _settings.RpcUrl);

                // Get contract and function
                var contract = web3.Eth.GetContract(RecallRegistryABI, _settings.ContractAddress);
                var function = contract.GetFunction(functionName);

                if (function == null)
                {
                    throw new InvalidOperationException($"Function {functionName} not found in contract ABI");
                }

                _logger.LogInformation("Function {FunctionName} retrieved from contract", functionName);

                // Get gas limit
                ulong gasLimit = await EstimateGasLimitAsync(function, parameters);

                // Send transaction
                _logger.LogInformation("Sending transaction with gas limit: {GasLimit}", gasLimit);
                var receipt = await function.SendTransactionAndWaitForReceiptAsync(
                    from: account.Address,
                    gas: new Nethereum.Hex.HexTypes.HexBigInteger(gasLimit),
                    value: null,
                    functionInput: parameters);

                if (receipt == null)
                {
                    _logger.LogError("Transaction returned null receipt for function {FunctionName}", functionName);
                    throw new InvalidOperationException("Transaction receipt is null - transaction may have failed");
                }

                // CRITICAL: Check if transaction was successful on-chain
                if (receipt.Status?.Value != 1)
                {
                    _logger.LogError(
                        "Transaction FAILED on blockchain. TxHash: {TxHash}, BlockNumber: {BlockNumber}, GasUsed: {GasUsed}, Status: {Status}",
                        receipt.TransactionHash, receipt.BlockNumber?.Value, receipt.GasUsed?.Value, receipt.Status?.Value);

                    throw new InvalidOperationException(
                        $"Transaction failed on blockchain. TxHash: {receipt.TransactionHash}, Status: {receipt.Status?.Value}");
                }

                _logger.LogInformation(
                    "Transaction successful. TxHash: {TxHash}, BlockNumber: {BlockNumber}, GasUsed: {GasUsed}, Status: {Status}",
                    receipt.TransactionHash, receipt.BlockNumber?.Value, receipt.GasUsed?.Value, receipt.Status?.Value);

                return receipt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending transaction for function {FunctionName}", functionName);
                throw;
            }
        }

        /// <summary>
        /// Formats the private key to ensure it's valid for Nethereum
        /// </summary>
        private string FormatPrivateKey(string privateKey)
        {
            if (string.IsNullOrWhiteSpace(privateKey))
            {
                throw new ArgumentException("Private key cannot be empty");
            }

            var cleanKey = privateKey.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? privateKey.Substring(2)
                : privateKey;

            if (!System.Text.RegularExpressions.Regex.IsMatch(cleanKey, @"^[0-9a-fA-F]+$"))
            {
                throw new ArgumentException("Private key must be valid hexadecimal");
            }

            if (cleanKey.Length != 64)
            {
                throw new ArgumentException($"Private key must be 64 hexadecimal characters (got {cleanKey.Length})");
            }

            return "0x" + cleanKey;
        }

        /// <summary>
        /// Validates blockchain settings
        /// </summary>
        private void ValidateBlockchainSettings()
        {
            if (string.IsNullOrWhiteSpace(_settings.RpcUrl))
            {
                throw new InvalidOperationException("RPC URL is not configured");
            }

            if (string.IsNullOrWhiteSpace(_settings.ContractAddress))
            {
                throw new InvalidOperationException("Contract address is not configured");
            }

            if (string.IsNullOrWhiteSpace(_settings.PrivateKey))
            {
                throw new InvalidOperationException("Private key is not configured");
            }

            if (_settings.ChainId <= 0)
            {
                throw new InvalidOperationException("Chain ID is not valid");
            }
        }

        /// <summary>
        /// Validates input parameters
        /// </summary>
        private void ValidateInputs(string actorRole, string actor, string? status = null)
        {
            if (string.IsNullOrWhiteSpace(actorRole))
            {
                throw new ArgumentException("Actor role cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(actor))
            {
                throw new ArgumentException("Actor cannot be null or empty");
            }

            if (status != null && string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Status cannot be null or empty when provided");
            }
        }
    }
}