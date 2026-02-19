using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.Models;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.DTOs;
using System.Linq;

namespace NotiBlock.Backend.Services
{
    public class RecallService(
        AppDbContext context,
        IBlockchainService blockchainService,
        INotificationService notificationService,
        ILogger<RecallService> logger) : IRecallService
    {
        private readonly AppDbContext _context = context;
        private readonly IBlockchainService _blockchainService = blockchainService;
        private readonly INotificationService _notificationService = notificationService;
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

            // Prevent duplicate active or pending recalls for the same product
            var existingRecall = await _context.Recalls
                .AnyAsync(r => r.ProductSerialNumber == product.SerialNumber &&
                               (r.Status == RecallStatus.Active || r.Status == RecallStatus.PendingApproval) &&
                               !r.IsDeleted);

            if (existingRecall)
            {
                _logger.LogWarning("Manufacturer {ManufacturerId} attempted to create duplicate recall for product {ProductId}",
                    manufacturerId, dto.ProductId);
                throw new InvalidOperationException($"A recall already exists for product {dto.ProductId}. Await regulator decision or resolve the active recall before creating another.");
            }

            var recall = new Recall
            {
                ManufacturerId = manufacturerId,
                ProductSerialNumber = product.SerialNumber,
                Reason = dto.Reason.Trim(),
                ActionRequired = dto.ActionRequired.Trim(),
                Status = RecallStatus.PendingApproval,
                IssuedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.Recalls.Add(recall);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Recall {RecallId} submitted for approval by manufacturer {ManufacturerId}",
                recall.Id, manufacturerId);

            await NotifyRegulatorsOfPendingRecallAsync(recall.Id);
            await NotifyManufacturerRecallSubmittedAsync(recall.Id, manufacturerId);

            return await GetRecallResponseDto(recall.Id);
        }

        // ===== BLOCKCHAIN INTEGRATION METHODS =====

        public async Task<RecallBlockchainDTO> IssueRecallToBlockchainAsync(Guid recallId, Guid regulatorId, string? regulatorNotes = null)
        {
            try
            {
                await EnsureRegulatorExistsAsync(regulatorId);

                var recall = await _context.Recalls
                    .Include(r => r.Manufacturer)
                    .FirstOrDefaultAsync(r => r.Id == recallId)
                    ?? throw new KeyNotFoundException($"Recall {recallId} not found");

                if (recall.Status != RecallStatus.PendingApproval)
                {
                    _logger.LogWarning("Recall {RecallId} is in status {Status} and cannot be approved", recallId, recall.Status);
                    throw new InvalidOperationException("Only recalls pending approval can be activated");
                }

                var existingBlockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.RecallId == recallId);

                if (existingBlockchainRecall != null)
                {
                    _logger.LogWarning("Recall {RecallId} already issued to blockchain", recallId);
                    throw new InvalidOperationException("Recall already issued to blockchain");
                }

                _logger.LogInformation("Regulator {RegulatorId} approving recall {RecallId}", regulatorId, recallId);

                var blockchainData = await _blockchainService.EmitRecallIssuedAsync(
                    recallId,
                    actorRole: "regulator",
                    actor: regulatorId.ToString());

                recall.TransactionHash = blockchainData.TransactionHash;
                recall.Status = RecallStatus.Active;
                recall.ApprovedBy = regulatorId;
                recall.ApprovedAt = DateTime.UtcNow;
                recall.RegulatorNotes = string.IsNullOrWhiteSpace(regulatorNotes) ? recall.RegulatorNotes : regulatorNotes.Trim();
                recall.RejectedAt = null;
                recall.RejectedBy = null;
                recall.IssuedAt = DateTime.UtcNow;
                recall.LastUpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Recall {RecallId} approved and recorded on blockchain. TxHash: {TxHash}",
                    recallId, blockchainData.TransactionHash);

                await NotifyManufacturerRecallApprovedAsync(recall.Id, recall.ManufacturerId);
                await SendRecallActivatedNotificationsAsync(recall.Id);

                return blockchainData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing recall {RecallId} to blockchain", recallId);
                throw;
            }
        }

        public async Task<RecallBlockchainDTO> UpdateRecallStatusOnBlockchainAsync(Guid recallId, string newStatus, Guid regulatorId, string? regulatorNotes = null)
        {
            try
            {
                await EnsureRegulatorExistsAsync(regulatorId);

                var recall = await _context.Recalls.FindAsync(recallId)
                    ?? throw new KeyNotFoundException($"Recall {recallId} not found");

                var blockchainRecall = await _context.BlockchainRecalls
                    .FirstOrDefaultAsync(br => br.RecallId == recallId)
                    ?? throw new InvalidOperationException("Recall not issued to blockchain yet. Issue it first.");

                _logger.LogInformation("Regulator {RegulatorId} updating recall {RecallId} status to {NewStatus} on blockchain", regulatorId, recallId, newStatus);

                var blockchainData = await _blockchainService.EmitRecallStatusChangedAsync(
                    recallId,
                    newStatus: newStatus,
                    actorRole: "regulator",
                    actor: regulatorId.ToString());

                recall.TransactionHash = blockchainData.TransactionHash;
                recall.LastUpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(regulatorNotes))
                {
                    recall.RegulatorNotes = regulatorNotes.Trim();
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Recall {RecallId} status updated to {NewStatus} on blockchain. TxHash: {TxHash}",
                    recallId, newStatus, blockchainData.TransactionHash);

                await SendRecallStatusUpdatedNotificationsAsync(recallId, newStatus);

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
                .Include(r => r.UpdateRequests)
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
                .Include(r => r.UpdateRequests)
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
                .Include(r => r.UpdateRequests)
                .Where(r => r.ProductSerialNumber == productId.Trim().ToUpperInvariant() && !r.IsDeleted)
                .OrderByDescending(r => r.IssuedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} recalls for product {ProductId}",
                recalls.Count, productId);

            return recalls.Select(r => MapToResponseDto(r));
        }

        public async Task<IEnumerable<RecallResponseDTO>> GetPendingRecallsForApprovalAsync()
        {
            var recalls = await _context.Recalls
                .Include(r => r.Manufacturer)
                .Include(r => r.UpdateRequests)
                .Where(r => r.Status == RecallStatus.PendingApproval && !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} pending recalls for regulator review", recalls.Count);

            return recalls.Select(MapToResponseDto);
        }

        public async Task<RecallResponseDTO?> UpdateRecallAsync(Guid id, RecallUpdateDTO dto, Guid regulatorId)
        {
            var recall = await _context.Recalls
                .Include(r => r.Manufacturer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recall == null || recall.IsDeleted)
            {
                _logger.LogWarning("Attempted to update non-existent or deleted recall: {RecallId}", id);
                return null;
            }

            await EnsureRegulatorExistsAsync(regulatorId);

            var oldStatus = recall.Status;
            var statusChanged = false;

            if (!string.IsNullOrWhiteSpace(dto.Reason))
            {
                recall.Reason = dto.Reason.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.ActionRequired))
            {
                recall.ActionRequired = dto.ActionRequired.Trim();
            }

            if (dto.Status.HasValue && dto.Status.Value != oldStatus)
            {
                statusChanged = true;
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

            _logger.LogInformation("Recall {RecallId} updated by regulator {UserId}", id, regulatorId);

            if (statusChanged)
            {
                await SendRecallStatusUpdatedNotificationsAsync(id, recall.Status.ToString());
            }

            return await GetRecallResponseDto(id);
        }

        public async Task<bool> SoftDeleteRecallAsync(Guid id, Guid actorId, bool isRegulator)
        {
            var recall = await _context.Recalls.FindAsync(id);

            if (recall == null || recall.IsDeleted)
            {
                _logger.LogWarning("Attempted to delete non-existent or already deleted recall: {RecallId}", id);
                return false;
            }

            if (!isRegulator)
            {
                if (recall.ManufacturerId != actorId)
                {
                    _logger.LogWarning("Manufacturer {UserId} attempted to delete recall {RecallId} owned by manufacturer {ManufacturerId}",
                        actorId, id, recall.ManufacturerId);
                    throw new UnauthorizedAccessException("You can only delete your own recalls");
                }

                if (recall.Status != RecallStatus.PendingApproval)
                {
                    _logger.LogWarning("Manufacturer {UserId} attempted to delete recall {RecallId} in status {Status}",
                        actorId, id, recall.Status);
                    throw new InvalidOperationException("Only recalls pending approval can be withdrawn by manufacturers");
                }
            }

            recall.IsDeleted = true;
            recall.DeletedAt = DateTime.UtcNow;
            recall.DeletedBy = actorId;
            recall.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Recall {RecallId} soft deleted by {Role} {UserId}",
                id, isRegulator ? "regulator" : "manufacturer", actorId);

            return true;
        }

        public async Task<bool> ResolveRecallAsync(Guid id, Guid regulatorId)
        {
            await EnsureRegulatorExistsAsync(regulatorId);

            var recall = await _context.Recalls.FindAsync(id);

            if (recall == null || recall.IsDeleted)
            {
                _logger.LogWarning("Attempted to resolve non-existent or deleted recall: {RecallId}", id);
                return false;
            }

            if (recall.Status == RecallStatus.PendingApproval)
            {
                _logger.LogWarning("Recall {RecallId} is still pending approval and cannot be resolved", id);
                throw new InvalidOperationException("Cannot resolve a recall that has not been activated");
            }

            recall.Status = RecallStatus.Resolved;
            recall.ResolvedAt = DateTime.UtcNow;
            recall.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Recall {RecallId} resolved by regulator {UserId}", id, regulatorId);

            await UpdateRecallStatusOnBlockchainAsync(id, RecallStatus.Resolved.ToString(), regulatorId);
            await SendRecallResolvedNotificationsAsync(id);

            return true;
        }

        public async Task<RecallResponseDTO> ApproveRecallAsync(Guid recallId, Guid regulatorId, RecallApprovalDTO dto)
        {
            await IssueRecallToBlockchainAsync(recallId, regulatorId, dto.Notes);
            return await GetRecallResponseDto(recallId);
        }

        public async Task<RecallResponseDTO> RejectRecallAsync(Guid recallId, Guid regulatorId, RecallRejectionDTO dto)
        {
            await EnsureRegulatorExistsAsync(regulatorId);

            var recall = await _context.Recalls.FindAsync(recallId)
                ?? throw new KeyNotFoundException($"Recall {recallId} not found");

            if (recall.Status != RecallStatus.PendingApproval)
            {
                _logger.LogWarning("Recall {RecallId} is in status {Status} and cannot be rejected", recallId, recall.Status);
                throw new InvalidOperationException("Only recalls pending approval can be rejected");
            }

            recall.Status = RecallStatus.Rejected;
            recall.RejectedBy = regulatorId;
            recall.RejectedAt = DateTime.UtcNow;
            recall.RegulatorNotes = dto.Reason.Trim();
            recall.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await NotifyManufacturerRecallRejectedAsync(recallId, recall.ManufacturerId, dto.Reason);

            return await GetRecallResponseDto(recallId);
        }

        public async Task<RecallUpdateRequestResponseDTO> CreateUpdateRequestAsync(Guid recallId, RecallUpdateProposalDTO dto, Guid manufacturerId)
        {
            var recall = await _context.Recalls
                .Include(r => r.Manufacturer)
                .FirstOrDefaultAsync(r => r.Id == recallId)
                ?? throw new KeyNotFoundException($"Recall {recallId} not found");

            if (recall.ManufacturerId != manufacturerId)
            {
                _logger.LogWarning("Manufacturer {UserId} attempted to update recall {RecallId} they do not own", manufacturerId, recallId);
                throw new UnauthorizedAccessException("You can only propose updates for your own recalls");
            }

            if (recall.Status == RecallStatus.PendingApproval)
            {
                throw new InvalidOperationException("Recall is already pending approval. Edit the proposal instead of creating an update request.");
            }

            if (recall.Status == RecallStatus.Rejected)
            {
                throw new InvalidOperationException("Rejected recalls cannot be updated");
            }

            if (string.IsNullOrWhiteSpace(dto.Reason) && string.IsNullOrWhiteSpace(dto.ActionRequired) && !dto.Status.HasValue)
            {
                throw new InvalidOperationException("Provide at least one field to update");
            }

            var pendingRequestExists = await _context.RecallUpdateRequests
                .AnyAsync(r => r.RecallId == recallId && r.Status == RecallUpdateRequestStatus.Pending && !r.IsDeleted);

            if (pendingRequestExists)
            {
                throw new InvalidOperationException("There is already a pending update request for this recall");
            }

            var request = new RecallUpdateRequest
            {
                RecallId = recallId,
                ManufacturerId = manufacturerId,
                ProposedReason = dto.Reason?.Trim(),
                ProposedActionRequired = dto.ActionRequired?.Trim(),
                ProposedStatus = dto.Status,
                ManufacturerNotes = dto.Notes?.Trim()
            };

            _context.RecallUpdateRequests.Add(request);
            await _context.SaveChangesAsync();

            await NotifyRegulatorsOfUpdateRequestAsync(request.Id);

            return MapToUpdateRequestResponseDto(request, recall.Manufacturer?.CompanyName);
        }

        public async Task<IEnumerable<RecallUpdateRequestResponseDTO>> GetPendingUpdateRequestsAsync()
        {
            var requests = await _context.RecallUpdateRequests
                .Include(r => r.Manufacturer)
                .Include(r => r.Recall)
                .Where(r => r.Status == RecallUpdateRequestStatus.Pending && !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(r => MapToUpdateRequestResponseDto(r, r.Manufacturer?.CompanyName));
        }

        public async Task<RecallResponseDTO> DecideUpdateRequestAsync(Guid requestId, RecallUpdateDecisionDTO dto, Guid regulatorId)
        {
            await EnsureRegulatorExistsAsync(regulatorId);

            var request = await _context.RecallUpdateRequests
                .Include(r => r.Recall)
                .Include(r => r.Manufacturer)
                .FirstOrDefaultAsync(r => r.Id == requestId)
                ?? throw new KeyNotFoundException($"Update request {requestId} not found");

            if (request.Status != RecallUpdateRequestStatus.Pending)
            {
                throw new InvalidOperationException("This update request has already been reviewed");
            }

            request.Status = dto.Approve ? RecallUpdateRequestStatus.Approved : RecallUpdateRequestStatus.Rejected;
            request.ReviewedBy = regulatorId;
            request.ReviewedAt = DateTime.UtcNow;
            request.RegulatorNotes = dto.Notes?.Trim();
            request.LastUpdatedAt = DateTime.UtcNow;

            if (dto.Approve)
            {
                var recall = request.Recall ?? throw new InvalidOperationException("Recall not loaded for update request");

                var originalStatus = recall.Status;

                if (!string.IsNullOrWhiteSpace(request.ProposedReason))
                {
                    recall.Reason = request.ProposedReason;
                }

                if (!string.IsNullOrWhiteSpace(request.ProposedActionRequired))
                {
                    recall.ActionRequired = request.ProposedActionRequired;
                }

                if (request.ProposedStatus.HasValue && request.ProposedStatus.Value != recall.Status)
                {
                    recall.Status = request.ProposedStatus.Value;

                    if (recall.Status == RecallStatus.Resolved)
                    {
                        recall.ResolvedAt ??= DateTime.UtcNow;
                    }
                }

                recall.LastUpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await UpdateRecallStatusOnBlockchainAsync(recall.Id, recall.Status.ToString(), regulatorId, request.RegulatorNotes);

                if (recall.Status == RecallStatus.Resolved && originalStatus != RecallStatus.Resolved)
                {
                    await SendRecallResolvedNotificationsAsync(recall.Id);
                }
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            await NotifyManufacturerUpdateDecisionAsync(request);

            return await GetRecallResponseDto(request.RecallId);
        }

        private async Task EnsureRegulatorExistsAsync(Guid regulatorId)
        {
            var exists = await _context.Regulators
                .AnyAsync(r => r.Id == regulatorId && !r.IsDeleted);

            if (!exists)
            {
                _logger.LogWarning("Regulator {RegulatorId} not found", regulatorId);
                throw new UnauthorizedAccessException("Regulator not found or inactive");
            }
        }

        // ===== NOTIFICATION HELPER METHODS =====

        private async Task SendRecallActivatedNotificationsAsync(Guid recallId)
        {
            var recall = await _context.Recalls
                .FirstOrDefaultAsync(r => r.Id == recallId);

            if (recall == null) return;

            var notifications = new List<NotificationCreateDTO>();

            // 1. Notify manufacturer (confirmation)
            notifications.Add(new NotificationCreateDTO
            {
                RecipientId = recall.ManufacturerId,
                RecipientType = "manufacturer",
                Type = NotificationType.Info,
                Title = "Recall Activated by Regulator",
                Message = $"A regulator approved your recall for product {recall.ProductSerialNumber}. Reason: {recall.Reason}",
                RelatedEntityId = recallId,
                RelatedEntityType = "recall",
                Priority = NotificationPriority.Normal
            });

            // 2. Notify affected consumers (products with same model)
            var affectedConsumers = await _context.Products
                .Where(p => p.Model == recall.ProductSerialNumber && p.OwnerId.HasValue && !p.IsDeleted)
                .Select(p => new { p.OwnerId, p.SerialNumber })
                .Distinct()
                .ToListAsync();

            foreach (var consumer in affectedConsumers)
            {
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = consumer.OwnerId!.Value,
                    RecipientType = "consumer",
                    Type = NotificationType.RecallIssued,
                    Title = "CRITICAL: Product Recall Alert",
                    Message = $"A recall has been issued for your product ({consumer.SerialNumber}). Reason: {recall.Reason}. Action required: {recall.ActionRequired}",
                    RelatedEntityId = recallId,
                    RelatedEntityType = "recall",
                    Priority = NotificationPriority.Critical,
                    ExpiresAt = DateTime.UtcNow.AddDays(90)
                });
            }

            // 3. Notify resellers who distribute this product model
            var affectedResellers = await _context.Products
                .Where(p => p.Model == recall.ProductSerialNumber && p.ResellerId.HasValue && !p.IsDeleted)
                .Select(p => p.ResellerId!.Value)
                .Distinct()
                .ToListAsync();

            foreach (var resellerId in affectedResellers)
            {
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = resellerId,
                    RecipientType = "reseller",
                    Type = NotificationType.RecallIssued,
                    Title = "Product Recall Alert",
                    Message = $"A recall has been issued for product model {recall.ProductSerialNumber} that you distribute. Reason: {recall.Reason}. Please inform your customers.",
                    RelatedEntityId = recallId,
                    RelatedEntityType = "recall",
                    Priority = NotificationPriority.High,
                    ExpiresAt = DateTime.UtcNow.AddDays(90)
                });
            }

            // 4. Notify all regulators (oversight)
            var regulators = await _context.Regulators
                .Where(r => !r.IsDeleted)
                .Select(r => r.Id)
                .ToListAsync();

            foreach (var regulatorId in regulators)
            {
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = regulatorId,
                    RecipientType = "regulator",
                    Type = NotificationType.Info,
                    Title = "New Recall Issued",
                    Message = $"A recall for product {recall.ProductSerialNumber} is now active. Reason: {recall.Reason}",
                    RelatedEntityId = recallId,
                    RelatedEntityType = "recall",
                    Priority = NotificationPriority.Normal
                });
            }

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Sent {Count} notifications for recall {RecallId} activation",
                    notifications.Count, recallId);
            }
        }

        private async Task NotifyRegulatorsOfPendingRecallAsync(Guid recallId)
        {
            var recall = await _context.Recalls
                .Include(r => r.Manufacturer)
                .FirstOrDefaultAsync(r => r.Id == recallId);

            if (recall == null) return;

            var regulators = await _context.Regulators
                .Where(r => !r.IsDeleted)
                .Select(r => r.Id)
                .ToListAsync();

            if (regulators.Count == 0) return;

            var notifications = regulators.Select(regulatorId => new NotificationCreateDTO
            {
                RecipientId = regulatorId,
                RecipientType = "regulator",
                Type = NotificationType.Warning,
                Title = "Recall Approval Needed",
                Message = $"Manufacturer {recall.ManufacturerId} submitted a recall for product {recall.ProductSerialNumber}.",
                RelatedEntityId = recallId,
                RelatedEntityType = "recall",
                Priority = NotificationPriority.High
            }).ToList();

            await _notificationService.CreateBulkNotificationsAsync(notifications);
        }

        private async Task NotifyManufacturerRecallSubmittedAsync(Guid recallId, Guid manufacturerId)
        {
            await _notificationService.CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = manufacturerId,
                RecipientType = "manufacturer",
                Type = NotificationType.Info,
                Title = "Recall Submitted",
                Message = "Your recall proposal has been sent to regulators for approval.",
                RelatedEntityId = recallId,
                RelatedEntityType = "recall",
                Priority = NotificationPriority.Normal
            });
        }

        private async Task NotifyManufacturerRecallApprovedAsync(Guid recallId, Guid manufacturerId)
        {
            await _notificationService.CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = manufacturerId,
                RecipientType = "manufacturer",
                Type = NotificationType.Info,
                Title = "Recall Approved",
                Message = "A regulator activated your recall. All stakeholders have been notified.",
                RelatedEntityId = recallId,
                RelatedEntityType = "recall",
                Priority = NotificationPriority.High
            });
        }

        private async Task NotifyManufacturerRecallRejectedAsync(Guid recallId, Guid manufacturerId, string reason)
        {
            await _notificationService.CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = manufacturerId,
                RecipientType = "manufacturer",
                Type = NotificationType.Warning,
                Title = "Recall Rejected",
                Message = reason,
                RelatedEntityId = recallId,
                RelatedEntityType = "recall",
                Priority = NotificationPriority.Normal
            });
        }

        private async Task NotifyRegulatorsOfUpdateRequestAsync(Guid requestId)
        {
            var request = await _context.RecallUpdateRequests
                .Include(r => r.Recall)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return;

            var regulators = await _context.Regulators
                .Where(r => !r.IsDeleted)
                .Select(r => r.Id)
                .ToListAsync();

            if (regulators.Count == 0) return;

            var notifications = regulators.Select(regulatorId => new NotificationCreateDTO
            {
                RecipientId = regulatorId,
                RecipientType = "regulator",
                Type = NotificationType.Warning,
                Title = "Recall Update Pending",
                Message = $"A manufacturer requested updates for recall {request.Recall?.ProductSerialNumber}.",
                RelatedEntityId = request.RecallId,
                RelatedEntityType = "recall",
                Priority = NotificationPriority.High
            }).ToList();

            await _notificationService.CreateBulkNotificationsAsync(notifications);
        }

        private async Task NotifyManufacturerUpdateDecisionAsync(RecallUpdateRequest request)
        {
            var message = request.Status == RecallUpdateRequestStatus.Approved
                ? "Regulators approved your recall update request."
                : "Regulators rejected your recall update request.";

            await _notificationService.CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = request.ManufacturerId,
                RecipientType = "manufacturer",
                Type = request.Status == RecallUpdateRequestStatus.Approved ? NotificationType.Info : NotificationType.Warning,
                Title = "Recall Update Decision",
                Message = string.IsNullOrWhiteSpace(request.RegulatorNotes) ? message : request.RegulatorNotes!,
                RelatedEntityId = request.RecallId,
                RelatedEntityType = "recall",
                Priority = NotificationPriority.Normal
            });
        }

        private async Task SendRecallStatusUpdatedNotificationsAsync(Guid recallId, string newStatus)
        {
            var recall = await _context.Recalls
                .FirstOrDefaultAsync(r => r.Id == recallId);

            if (recall == null) return;

            // Notify affected consumers
            var affectedConsumers = await _context.Products
                .Where(p => p.Model == recall.ProductSerialNumber && p.OwnerId.HasValue && !p.IsDeleted)
                .Select(p => new { p.OwnerId, p.SerialNumber })
                .Distinct()
                .ToListAsync();

            var notifications = affectedConsumers.Select(consumer => new NotificationCreateDTO
            {
                RecipientId = consumer.OwnerId!.Value,
                RecipientType = "consumer",
                Type = NotificationType.Info,
                Title = "Recall Status Updated",
                Message = $"The recall for your product ({consumer.SerialNumber}) status has been updated to: {newStatus}",
                RelatedEntityId = recallId,
                RelatedEntityType = "recall",
                Priority = NotificationPriority.High
            }).ToList();

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Notified {Count} consumers about recall {RecallId} status update to {Status}",
                    notifications.Count, recallId, newStatus);
            }
        }

        private async Task SendRecallResolvedNotificationsAsync(Guid recallId)
        {
            var recall = await _context.Recalls
                .FirstOrDefaultAsync(r => r.Id == recallId);

            if (recall == null) return;

            // Notify affected consumers
            var affectedConsumers = await _context.Products
                .Where(p => p.Model == recall.ProductSerialNumber && p.OwnerId.HasValue && !p.IsDeleted)
                .Select(p => new { p.OwnerId, p.SerialNumber })
                .Distinct()
                .ToListAsync();

            var notifications = affectedConsumers.Select(consumer => new NotificationCreateDTO
            {
                RecipientId = consumer.OwnerId!.Value,
                RecipientType = "consumer",
                Type = NotificationType.Info,
                Title = "Recall Resolved",
                Message = $"Good news! The recall for your product ({consumer.SerialNumber}) has been marked as resolved.",
                RelatedEntityId = recallId,
                RelatedEntityType = "recall",
                Priority = NotificationPriority.High
            }).ToList();

            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
                _logger.LogInformation("Notified {Count} consumers about recall {RecallId} resolution",
                    notifications.Count, recallId);
            }
        }

        private async Task<RecallResponseDTO> GetRecallResponseDto(Guid id)
        {
            var recall = await _context.Recalls
                .Include(r => r.Manufacturer)
                .Include(r => r.UpdateRequests)
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
                ApprovedBy = recall.ApprovedBy,
                ApprovedAt = recall.ApprovedAt,
                RejectedBy = recall.RejectedBy,
                RejectedAt = recall.RejectedAt,
                RegulatorNotes = recall.RegulatorNotes,
                PendingUpdateRequestCount = recall.UpdateRequests.Count(r => !r.IsDeleted && r.Status == RecallUpdateRequestStatus.Pending),
                CreatedAt = recall.CreatedAt,
                LastUpdatedAt = recall.LastUpdatedAt
            };
        }

        private static RecallUpdateRequestResponseDTO MapToUpdateRequestResponseDto(RecallUpdateRequest request, string? manufacturerName)
        {
            return new RecallUpdateRequestResponseDTO
            {
                Id = request.Id,
                RecallId = request.RecallId,
                ManufacturerId = request.ManufacturerId,
                ManufacturerName = manufacturerName ?? string.Empty,
                ProposedReason = request.ProposedReason,
                ProposedActionRequired = request.ProposedActionRequired,
                ProposedStatus = request.ProposedStatus,
                ManufacturerNotes = request.ManufacturerNotes,
                Status = request.Status,
                ReviewedBy = request.ReviewedBy,
                ReviewedAt = request.ReviewedAt,
                RegulatorNotes = request.RegulatorNotes,
                CreatedAt = request.CreatedAt,
                LastUpdatedAt = request.LastUpdatedAt
            };
        }
    }
}