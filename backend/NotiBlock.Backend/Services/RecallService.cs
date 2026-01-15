using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.Models;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.DTOs;

namespace NotiBlock.Backend.Services
{
    public class RecallService(
        AppDbContext context,
        IBlockchainService blockchainService,
        ILogger<RecallService> logger) : IRecallService
    {
        private readonly AppDbContext _context = context;
        private readonly IBlockchainService _blockchainService = blockchainService;
        private readonly ILogger<RecallService> _logger = logger;

        public async Task<RecallResponseDTO> CreateRecallAsync(RecallCreateDTO dto, Guid manufacturerId)
        {
            var manufacturer = await _context.Manufacturers
                .FirstOrDefaultAsync(m => m.Id == manufacturerId && !m.IsDeleted);

            if (manufacturer == null)
            {
                _logger.LogWarning("Attempted to create recall for non-existent manufacturer: {ManufacturerId}",
                    manufacturerId);
                throw new KeyNotFoundException("Manufacturer not found");
            }

            // Validate product exists and belongs to the manufacturer
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SerialNumber == dto.ProductId.Trim().ToUpperInvariant() && !p.IsDeleted);

            if (product == null)
            {
                _logger.LogWarning("Manufacturer {ManufacturerId} attempted to create recall for non-existent product: {ProductId}",
                    manufacturerId, dto.ProductId);
                throw new KeyNotFoundException($"Product with serial number {dto.ProductId} not found");
            }

            if (product.ManufacturerId != manufacturerId)
            {
                _logger.LogWarning("Manufacturer {ManufacturerId} attempted to create recall for product {ProductId} owned by manufacturer {ProductManufacturerId}",
                    manufacturerId, dto.ProductId, product.ManufacturerId);
                throw new UnauthorizedAccessException("You can only create recalls for products you manufactured");
            }

            // Check for existing active recall for this product
            var existingRecall = await _context.Recalls
                .AnyAsync(r => r.ProductSerialNumber == product.SerialNumber &&
                              r.Status == RecallStatus.Active &&
                              !r.IsDeleted);

            if (existingRecall)
            {
                _logger.LogWarning("Manufacturer {ManufacturerId} attempted to create duplicate active recall for product {ProductId}",
                    manufacturerId, dto.ProductId);
                throw new InvalidOperationException($"An active recall already exists for product {dto.ProductId}");
            }

            var recall = new Recall
            {
                ManufacturerId = manufacturerId,
                ProductSerialNumber = product.SerialNumber,
                Reason = dto.Reason.Trim(),
                ActionRequired = dto.ActionRequired.Trim(),
                Status = RecallStatus.Active,
                IssuedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.Recalls.Add(recall);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Recall {RecallId} created for product {ProductId} by manufacturer {ManufacturerId}",
                recall.Id, recall.ProductSerialNumber, manufacturerId);

            return await GetRecallResponseDto(recall.Id);
        }

        // ===== BLOCKCHAIN INTEGRATION METHODS =====

        public async Task<RecallBlockchainDTO> IssueRecallToBlockchainAsync(Guid recallId, Guid manufacturerId)
        {
            try
            {
                var recall = await _context.Recalls.FindAsync(recallId)
                    ?? throw new KeyNotFoundException($"Recall {recallId} not found");

                // Authorization: Only the manufacturer who created it can issue to blockchain
                if (recall.ManufacturerId != manufacturerId)
                {
                    _logger.LogWarning("Manufacturer {UserId} attempted to issue recall {RecallId} to blockchain (owned by {ManufacturerId})",
                        manufacturerId, recallId, recall.ManufacturerId);
                    throw new UnauthorizedAccessException("You can only issue your own recalls to blockchain");
                }

                // Check if already issued to blockchain
                var existingBlockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.RecallId == recallId);

                if (existingBlockchainRecall != null)
                {
                    _logger.LogWarning("Recall {RecallId} already issued to blockchain", recallId);
                    throw new InvalidOperationException("Recall already issued to blockchain");
                }

                _logger.LogInformation("Issuing recall {RecallId} to blockchain", recallId);

                // Emit RecallIssued event on blockchain
                var blockchainData = await _blockchainService.EmitRecallIssuedAsync(
                    recallId,
                    actorRole: "manufacturer",
                    actor: manufacturerId.ToString());

                // Update recall with transaction hash
                recall.TransactionHash = blockchainData.TransactionHash;
                recall.LastUpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Recall {RecallId} successfully issued to blockchain. TxHash: {TxHash}",
                    recallId, blockchainData.TransactionHash);

                return blockchainData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing recall {RecallId} to blockchain", recallId);
                throw;
            }
        }

        public async Task<RecallBlockchainDTO> UpdateRecallStatusOnBlockchainAsync(Guid recallId, string newStatus, Guid manufacturerId)
        {
            try
            {
                var recall = await _context.Recalls.FindAsync(recallId)
                    ?? throw new KeyNotFoundException($"Recall {recallId} not found");

                // Authorization: Only the manufacturer who created it can update
                if (recall.ManufacturerId != manufacturerId)
                {
                    _logger.LogWarning("Manufacturer {UserId} attempted to update recall {RecallId} on blockchain (owned by {ManufacturerId})",
                        manufacturerId, recallId, recall.ManufacturerId);
                    throw new UnauthorizedAccessException("You can only update your own recalls");
                }

                // Check if recall was issued to blockchain
                var blockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.RecallId == recallId)
                    ?? throw new InvalidOperationException("Recall not issued to blockchain yet. Issue it first.");

                _logger.LogInformation("Updating recall {RecallId} status to {NewStatus} on blockchain", recallId, newStatus);

                // Emit RecallStatusChanged event on blockchain
                var blockchainData = await _blockchainService.EmitRecallStatusChangedAsync(
                    recallId,
                    newStatus: newStatus,
                    actorRole: "manufacturer",
                    actor: manufacturerId.ToString());

                // Update recall status and transaction hash
                recall.TransactionHash = blockchainData.TransactionHash;
                recall.LastUpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Recall {RecallId} status updated to {NewStatus} on blockchain. TxHash: {TxHash}",
                    recallId, newStatus, blockchainData.TransactionHash);

                return blockchainData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recall {RecallId} status on blockchain", recallId);
                throw;
            }
        }

        public async Task<RecallBlockchainDTO?> GetRecallBlockchainDataAsync(Guid recallId)
        {
            try
            {
                var blockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.RecallId == recallId && !br.IsDeleted);

                if (blockchainRecall == null)
                {
                    _logger.LogInformation("No blockchain data found for recall {RecallId}", recallId);
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
                _logger.LogError(ex, "Error retrieving blockchain data for recall {RecallId}", recallId);
                return null;
            }
        }

        public async Task<bool> VerifyRecallOnBlockchainAsync(Guid recallId)
        {
            try
            {
                var blockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.RecallId == recallId && !br.IsDeleted);

                if (blockchainRecall == null)
                {
                    _logger.LogWarning("No blockchain record found for recall {RecallId}", recallId);
                    return false;
                }

                var isVerified = await _blockchainService.VerifyRecallOnBlockchainAsync(blockchainRecall.TransactionHash);

                _logger.LogInformation("Recall {RecallId} verification result: {IsVerified}", recallId, isVerified);

                return isVerified;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying recall {RecallId} on blockchain", recallId);
                return false;
            }
        }

        // ===== EXISTING METHODS =====

        public async Task<RecallResponseDTO?> GetRecallByIdAsync(Guid id)
        {
            try
            {
                return await GetRecallResponseDto(id);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Recall not found: {RecallId}", id);
                return null;
            }
        }

        public async Task<IEnumerable<RecallResponseDTO>> GetAllRecallsAsync(bool includeDeleted = false)
        {
            var query = _context.Recalls
                .Include(r => r.Manufacturer)
                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(r => !r.IsDeleted);
            }

            var recalls = await query
                .OrderByDescending(r => r.IssuedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} recalls (includeDeleted: {IncludeDeleted})",
                recalls.Count, includeDeleted);

            return recalls.Select(r => MapToResponseDto(r));
        }

        public async Task<IEnumerable<RecallResponseDTO>> GetRecallsByManufacturerAsync(Guid manufacturerId)
        {
            var recalls = await _context.Recalls
                .Include(r => r.Manufacturer)
                .Where(r => r.ManufacturerId == manufacturerId && !r.IsDeleted)
                .OrderByDescending(r => r.IssuedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} recalls for manufacturer {ManufacturerId}",
                recalls.Count, manufacturerId);

            return recalls.Select(r => MapToResponseDto(r));
        }

        public async Task<IEnumerable<RecallResponseDTO>> GetRecallsByProductAsync(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                _logger.LogWarning("Get recalls by product called with empty productId");
                throw new ArgumentException("Product ID cannot be empty");
            }

            var recalls = await _context.Recalls
                .Include(r => r.Manufacturer)
                .Where(r => r.ProductSerialNumber == productId.Trim().ToUpperInvariant() && !r.IsDeleted)
                .OrderByDescending(r => r.IssuedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} recalls for product {ProductId}",
                recalls.Count, productId);

            return recalls.Select(r => MapToResponseDto(r));
        }

        public async Task<RecallResponseDTO?> UpdateRecallAsync(Guid id, RecallUpdateDTO dto, Guid manufacturerId)
        {
            var recall = await _context.Recalls
                .Include(r => r.Manufacturer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recall == null || recall.IsDeleted)
            {
                _logger.LogWarning("Attempted to update non-existent or deleted recall: {RecallId}", id);
                return null;
            }

            // Authorization: Only the manufacturer who created it can update
            if (recall.ManufacturerId != manufacturerId)
            {
                _logger.LogWarning("Manufacturer {UserId} attempted to update recall {RecallId} owned by manufacturer {ManufacturerId}",
                    manufacturerId, id, recall.ManufacturerId);
                throw new UnauthorizedAccessException("You can only update your own recalls");
            }

            if (!string.IsNullOrWhiteSpace(dto.Reason))
            {
                recall.Reason = dto.Reason.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.ActionRequired))
            {
                recall.ActionRequired = dto.ActionRequired.Trim();
            }

            if (dto.Status.HasValue)
            {
                recall.Status = dto.Status.Value;
                if (dto.Status == RecallStatus.Resolved && recall.ResolvedAt == null)
                {
                    recall.ResolvedAt = DateTime.UtcNow;
                    _logger.LogInformation("Recall {RecallId} resolved at {ResolvedAt}", id, recall.ResolvedAt);
                }
                else if (dto.Status == RecallStatus.Cancelled)
                {
                    _logger.LogInformation("Recall {RecallId} cancelled", id);
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.TransactionHash))
            {
                recall.TransactionHash = dto.TransactionHash.Trim();
            }

            recall.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Recall {RecallId} updated by manufacturer {UserId}", id, manufacturerId);

            return await GetRecallResponseDto(id);
        }

        public async Task<bool> SoftDeleteRecallAsync(Guid id, Guid manufacturerId)
        {
            var recall = await _context.Recalls.FindAsync(id);

            if (recall == null || recall.IsDeleted)
            {
                _logger.LogWarning("Attempted to delete non-existent or already deleted recall: {RecallId}", id);
                return false;
            }

            if (recall.ManufacturerId != manufacturerId)
            {
                _logger.LogWarning("Manufacturer {UserId} attempted to delete recall {RecallId} owned by manufacturer {ManufacturerId}",
                    manufacturerId, id, recall.ManufacturerId);
                throw new UnauthorizedAccessException("You can only delete your own recalls");
            }

            recall.IsDeleted = true;
            recall.DeletedAt = DateTime.UtcNow;
            recall.DeletedBy = manufacturerId;
            recall.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Recall {RecallId} soft deleted by manufacturer {UserId}", id, manufacturerId);

            return true;
        }

        public async Task<bool> ResolveRecallAsync(Guid id, Guid manufacturerId)
        {
            var recall = await _context.Recalls.FindAsync(id);

            if (recall == null || recall.IsDeleted)
            {
                _logger.LogWarning("Attempted to resolve non-existent or deleted recall: {RecallId}", id);
                return false;
            }

            if (recall.ManufacturerId != manufacturerId)
            {
                _logger.LogWarning("Manufacturer {UserId} attempted to resolve recall {RecallId} owned by manufacturer {ManufacturerId}",
                    manufacturerId, id, recall.ManufacturerId);
                throw new UnauthorizedAccessException("You can only resolve your own recalls");
            }

            recall.Status = RecallStatus.Resolved;
            recall.ResolvedAt = DateTime.UtcNow;
            recall.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Recall {RecallId} resolved by manufacturer {UserId}", id, manufacturerId);

            return true;
        }

        private async Task<RecallResponseDTO> GetRecallResponseDto(Guid id)
        {
            var recall = await _context.Recalls
                .Include(r => r.Manufacturer)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted)
                ?? throw new KeyNotFoundException($"Recall {id} not found");

            return MapToResponseDto(recall);
        }

        private static RecallResponseDTO MapToResponseDto(Recall recall)
        {
            return new RecallResponseDTO
            {
                Id = recall.Id,
                ProductId = recall.ProductSerialNumber,
                Reason = recall.Reason,
                ActionRequired = recall.ActionRequired,
                Status = recall.Status,
                IssuedAt = recall.IssuedAt,
                ResolvedAt = recall.ResolvedAt,
                ManufacturerId = recall.ManufacturerId,
                ManufacturerName = recall.Manufacturer?.CompanyName ?? string.Empty,
                TransactionHash = recall.TransactionHash,
                CreatedAt = recall.CreatedAt,
                LastUpdatedAt = recall.LastUpdatedAt
            };
        }
    }
}