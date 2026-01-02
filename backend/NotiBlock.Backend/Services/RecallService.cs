using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.Models;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.DTOs.Recall;

namespace NotiBlock.Backend.Services
{
    public class RecallService(AppDbContext context, ILogger<RecallService> logger) : IRecallService
    {
        private readonly AppDbContext _context = context;
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

            return MapToResponseDto(recall);
        }

        public async Task<bool> SoftDeleteRecallAsync(Guid id, Guid manufacturerId)
        {
            var recall = await _context.Recalls
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recall == null)
            {
                _logger.LogWarning("Attempted to delete non-existent recall: {RecallId}", id);
                throw new KeyNotFoundException($"Recall with ID {id} not found");
            }

            if (recall.IsDeleted)
            {
                _logger.LogWarning("Attempted to delete already deleted recall: {RecallId}", id);
                throw new InvalidOperationException($"Recall {id} is already deleted");
            }

            // Authorization: Only the manufacturer who created it can delete
            if (recall.ManufacturerId != manufacturerId)
            {
                _logger.LogWarning("Manufacturer {UserId} attempted to delete recall {RecallId} owned by manufacturer {ManufacturerId}",
                    manufacturerId, id, recall.ManufacturerId);
                throw new UnauthorizedAccessException("You can only delete your own recalls");
            }

            // Only allow deletion if recall is in Active status
            if (recall.Status != RecallStatus.Active)
            {
                _logger.LogWarning("Attempted to delete recall {RecallId} with status {Status}",
                    id, recall.Status);
                throw new InvalidOperationException($"Cannot delete recall with status '{recall.Status}'. Use cancellation instead.");
            }

            recall.IsDeleted = true;
            recall.DeletedAt = DateTime.UtcNow;
            recall.DeletedBy = manufacturerId;
            recall.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Recall {RecallId} soft deleted by manufacturer {UserId} at {DeletedAt}",
                id, manufacturerId, recall.DeletedAt);

            return true;
        }

        public async Task<bool> ResolveRecallAsync(Guid id, Guid manufacturerId)
        {
            var recall = await _context.Recalls.FindAsync(id);

            if (recall == null || recall.IsDeleted)
            {
                _logger.LogWarning("Attempted to resolve non-existent or deleted recall: {RecallId}", id);
                throw new KeyNotFoundException($"Recall with ID {id} not found");
            }

            // Authorization: Only the manufacturer who created it can resolve
            if (recall.ManufacturerId != manufacturerId)
            {
                _logger.LogWarning("Manufacturer {UserId} attempted to resolve recall {RecallId} owned by manufacturer {ManufacturerId}",
                    manufacturerId, id, recall.ManufacturerId);
                throw new UnauthorizedAccessException("You can only resolve your own recalls");
            }

            if (recall.Status == RecallStatus.Resolved)
            {
                _logger.LogWarning("Attempted to resolve already resolved recall: {RecallId}", id);
                throw new InvalidOperationException("Recall is already resolved");
            }

            if (recall.Status == RecallStatus.Cancelled)
            {
                _logger.LogWarning("Attempted to resolve cancelled recall: {RecallId}", id);
                throw new InvalidOperationException("Cannot resolve a cancelled recall");
            }

            recall.Status = RecallStatus.Resolved;
            recall.ResolvedAt = DateTime.UtcNow;
            recall.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Recall {RecallId} resolved by manufacturer {UserId} at {ResolvedAt}",
                id, manufacturerId, recall.ResolvedAt);

            return true;
        }

        private async Task<RecallResponseDTO> GetRecallResponseDto(Guid id)
        {
            var recall = await _context.Recalls
                .Include(r => r.Manufacturer)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (recall == null)
            {
                throw new KeyNotFoundException("Recall not found");
            }

            return MapToResponseDto(recall);
        }

        private RecallResponseDTO MapToResponseDto(Recall recall)
        {
            return new RecallResponseDTO
            {
                Id = recall.Id,
                ManufacturerId = recall.ManufacturerId,
                ManufacturerName = recall.Manufacturer?.CompanyName ?? string.Empty,
                ProductId = recall.ProductSerialNumber,
                Reason = recall.Reason,
                ActionRequired = recall.ActionRequired,
                Status = recall.Status,
                IssuedAt = recall.IssuedAt,
                ResolvedAt = recall.ResolvedAt,
                TransactionHash = recall.TransactionHash,
                CreatedAt = recall.CreatedAt,
                LastUpdatedAt = recall.LastUpdatedAt
            };
        }
    }
}