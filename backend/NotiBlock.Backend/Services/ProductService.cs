using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services
{
    public class ProductService(AppDbContext context, ILogger<ProductService> logger, INotificationService notificationService) : IProductService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<ProductService> _logger = logger;
        private readonly INotificationService _notificationService = notificationService;

        // Constants for role checking
        private const string RoleConsumer = "consumer";
        private const string RoleReseller = "reseller";
        private const string RoleManufacturer = "manufacturer";

        public async Task<Product> CreateProductAsync(ProductCreateDTO dto, Guid manufacturerId)
        {
            // Check for duplicate serial number
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.SerialNumber == dto.SerialNumber.Trim());

            if (existingProduct != null)
            {
                _logger.LogWarning("Attempted to create product with duplicate serial number: {SerialNumber}", dto.SerialNumber);
                throw new ArgumentException($"Product with serial number {dto.SerialNumber} already exists");
            }

            var product = new Product
            {
                SerialNumber = dto.SerialNumber.Trim().ToUpperInvariant(),
                Model = dto.Model.Trim(),
                ManufacturerId = manufacturerId,
                RegisteredAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product created: {SerialNumber} by manufacturer {ManufacturerId}", 
                product.SerialNumber, manufacturerId);

            // ===== NOTIFICATION =====
            await _notificationService.CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = manufacturerId,
                RecipientType = "manufacturer",
                Type = NotificationType.Info,
                Title = "Product Created Successfully",
                Message = $"Product {product.SerialNumber} (Model: {product.Model}) has been created and added to your inventory.",
                RelatedEntityId = product.Id,
                RelatedEntityType = "product",
                Priority = NotificationPriority.Normal
            });

            return product;
        }

        public async Task<Product> RegisterProductAsync(ProductRegisterDTO dto, Guid registererId, string role)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SerialNumber == dto.SerialNumber.Trim())
                ?? throw new KeyNotFoundException($"Product with serial number {dto.SerialNumber} not found");

            if (role == RoleConsumer)
            {
                if (product.OwnerId.HasValue)
                {
                    _logger.LogWarning("Attempted to register already owned product {SerialNumber} to consumer {ConsumerId}", 
                        dto.SerialNumber, registererId);
                    throw new InvalidOperationException("Product already registered to a consumer");
                }

                // Validate consumer exists
                var consumerExists = await _context.Consumers.AnyAsync(c => c.Id == registererId && !c.IsDeleted);
                if (!consumerExists)
                {
                    _logger.LogWarning("Attempted to register product to non-existent consumer: {ConsumerId}", registererId);
                    throw new KeyNotFoundException($"Consumer with ID {registererId} not found");
                }

                product.OwnerId = registererId;
                product.RegisteredAt = DateTime.UtcNow;
                
                _logger.LogInformation("Product {SerialNumber} registered to consumer {ConsumerId}", 
                    dto.SerialNumber, registererId);

                await _context.SaveChangesAsync();
                
                // ===== NOTIFICATIONS =====
                var notifications = new List<NotificationCreateDTO>
                {
                    // Notify consumer
                    new NotificationCreateDTO
                    {
                        RecipientId = registererId,
                        RecipientType = "consumer",
                        Type = NotificationType.ProductRegistered,
                        Title = "Product Registered Successfully",
                        Message = $"Your product {product.SerialNumber} (Model: {product.Model}) has been successfully registered. You will receive notifications about any recalls affecting this product.",
                        RelatedEntityId = product.Id,
                        RelatedEntityType = "product",
                        Priority = NotificationPriority.Normal
                    }
                };

                // Notify manufacturer
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = product.ManufacturerId,
                    RecipientType = "manufacturer",
                    Type = NotificationType.Info,
                    Title = "Product Registered to Consumer",
                    Message = $"Product {product.SerialNumber} (Model: {product.Model}) has been registered to a consumer.",
                    RelatedEntityId = product.Id,
                    RelatedEntityType = "product",
                    Priority = NotificationPriority.Low
                });

                // Notify reseller if assigned
                if (product.ResellerId.HasValue)
                {
                    notifications.Add(new NotificationCreateDTO
                    {
                        RecipientId = product.ResellerId.Value,
                        RecipientType = "reseller",
                        Type = NotificationType.Info,
                        Title = "Product Sold to Consumer",
                        Message = $"Product {product.SerialNumber} (Model: {product.Model}) has been registered to a consumer.",
                        RelatedEntityId = product.Id,
                        RelatedEntityType = "product",
                        Priority = NotificationPriority.Normal
                    });
                }

                await _notificationService.CreateBulkNotificationsAsync(notifications);
            }
            else if (role == RoleReseller)
            {
                if (product.ResellerId.HasValue)
                {
                    _logger.LogWarning("Attempted to register already assigned product {SerialNumber} to reseller {ResellerId}", 
                        dto.SerialNumber, registererId);
                    throw new InvalidOperationException("Product already registered to a reseller");
                }

                // Validate reseller exists
                var resellerExists = await _context.Resellers.AnyAsync(r => r.Id == registererId && !r.IsDeleted);
                if (!resellerExists)
                {
                    _logger.LogWarning("Attempted to register product to non-existent reseller: {ResellerId}", registererId);
                    throw new KeyNotFoundException($"Reseller with ID {registererId} not found");
                }

                product.ResellerId = registererId;
                product.RegisteredAt = DateTime.UtcNow;
                
                _logger.LogInformation("Product {SerialNumber} registered to reseller {ResellerId}", 
                    dto.SerialNumber, registererId);

                await _context.SaveChangesAsync();

                // ===== NOTIFICATIONS =====
                var notifications = new List<NotificationCreateDTO>
                {
                    // Notify reseller
                    new NotificationCreateDTO
                    {
                        RecipientId = registererId,
                        RecipientType = "reseller",
                        Type = NotificationType.ProductRegistered,
                        Title = "Product Assigned to Your Inventory",
                        Message = $"Product {product.SerialNumber} (Model: {product.Model}) has been assigned to you for distribution.",
                        RelatedEntityId = product.Id,
                        RelatedEntityType = "product",
                        Priority = NotificationPriority.Normal
                    },
                    // Notify manufacturer
                    new NotificationCreateDTO
                    {
                        RecipientId = product.ManufacturerId,
                        RecipientType = "manufacturer",
                        Type = NotificationType.Info,
                        Title = "Product Assigned to Reseller",
                        Message = $"Product {product.SerialNumber} (Model: {product.Model}) has been assigned to a reseller for distribution.",
                        RelatedEntityId = product.Id,
                        RelatedEntityType = "product",
                        Priority = NotificationPriority.Low
                    }
                };

                await _notificationService.CreateBulkNotificationsAsync(notifications);
            }
            else
            {
                _logger.LogWarning("Invalid role {Role} attempted to register product {SerialNumber}", 
                    role, dto.SerialNumber);
                throw new InvalidOperationException($"Invalid role '{role}' for registration");
            }

            return product;
        }

        public async Task<Product> UnregisterProductAsync(ProductUnregisterDTO dto, Guid userId, string role)
        {
            if (string.IsNullOrWhiteSpace(dto.SerialNumber))
                throw new ArgumentException("Serial number cannot be empty");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SerialNumber == dto.SerialNumber.Trim())
                ?? throw new KeyNotFoundException($"Product with serial number {dto.SerialNumber} not found");

            switch (dto.Type)
            {
                case UnregisterType.RemoveReseller:
                    await HandleRemoveResellerAsync(product, userId, role);
                    break;

                case UnregisterType.RemoveConsumer:
                    await HandleRemoveConsumerAsync(product, userId, role);
                    break;

                case UnregisterType.SelfUnregister:
                    await HandleSelfUnregisterAsync(product, userId, role);
                    break;

                default:
                    throw new ArgumentException($"Invalid unregister type: {dto.Type}");
            }

            await _context.SaveChangesAsync();
            return product;
        }

        private async Task HandleRemoveResellerAsync(Product product, Guid userId, string role)
        {
            // Only manufacturers can remove reseller assignments
            if (role != RoleManufacturer)
            {
                _logger.LogWarning("Non-manufacturer {Role} {UserId} attempted to remove reseller from product {SerialNumber}",
                    role, userId, product.SerialNumber);
                throw new UnauthorizedAccessException("Only manufacturers can remove reseller assignments");
            }

            // Verify the manufacturer owns this product
            if (product.ManufacturerId != userId)
            {
                _logger.LogWarning("Manufacturer {UserId} attempted to unregister product {SerialNumber} owned by manufacturer {ManufacturerId}",
                    userId, product.SerialNumber, product.ManufacturerId);
                throw new UnauthorizedAccessException("You can only unregister products you created");
            }

            // Check if reseller is assigned
            if (!product.ResellerId.HasValue)
            {
                _logger.LogWarning("Attempted to remove reseller from product {SerialNumber} that has no reseller assigned",
                    product.SerialNumber);
                throw new InvalidOperationException("Product does not have a reseller assigned");
            }

            // Check if product has been sold to consumer
            if (product.OwnerId.HasValue)
            {
                _logger.LogWarning("Attempted to remove reseller from product {SerialNumber} that has already been sold to consumer",
                    product.SerialNumber);
                throw new InvalidOperationException("Cannot remove reseller from product that has been sold to a consumer");
            }

            var removedResellerId = product.ResellerId.Value;
            product.ResellerId = null;
            product.RegisteredAt = DateTime.UtcNow;

            _logger.LogInformation("Product {SerialNumber} unregistered from reseller {ResellerId} by manufacturer {ManufacturerId}",
                product.SerialNumber, removedResellerId, userId);

            // ===== NOTIFICATION =====
            await _notificationService.CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = removedResellerId,
                RecipientType = "reseller",
                Type = NotificationType.Warning,
                Title = "Product Removed from Inventory",
                Message = $"Product {product.SerialNumber} (Model: {product.Model}) has been removed from your inventory by the manufacturer.",
                RelatedEntityId = product.Id,
                RelatedEntityType = "product",
                Priority = NotificationPriority.High
            });
        }

        private async Task HandleRemoveConsumerAsync(Product product, Guid userId, string role)
        {
            // Manufacturers and resellers can remove consumer assignments
            if (role == RoleManufacturer)
            {
                // Verify the manufacturer owns this product
                if (product.ManufacturerId != userId)
                {
                    _logger.LogWarning("Manufacturer {UserId} attempted to unregister consumer from product {SerialNumber} owned by manufacturer {ManufacturerId}",
                        userId, product.SerialNumber, product.ManufacturerId);
                    throw new UnauthorizedAccessException("You can only unregister products you created");
                }
            }
            else if (role == RoleReseller)
            {
                // Verify the reseller is assigned to this product
                if (product.ResellerId != userId)
                {
                    _logger.LogWarning("Reseller {UserId} attempted to unregister consumer from product {SerialNumber} assigned to reseller {ResellerId}",
                        userId, product.SerialNumber, product.ResellerId);
                    throw new UnauthorizedAccessException("You can only unregister products assigned to you");
                }
            }
            else
            {
                _logger.LogWarning("Invalid role {Role} attempted to remove consumer from product {SerialNumber}",
                    role, product.SerialNumber);
                throw new UnauthorizedAccessException("Only manufacturers and resellers can remove consumer assignments");
            }

            // Check if consumer is assigned
            if (!product.OwnerId.HasValue)
            {
                _logger.LogWarning("Attempted to remove consumer from product {SerialNumber} that has no consumer assigned",
                    product.SerialNumber);
                throw new InvalidOperationException("Product does not have a consumer assigned");
            }

            var removedOwnerId = product.OwnerId.Value;
            product.OwnerId = null;
            product.RegisteredAt = DateTime.UtcNow;

            _logger.LogInformation("Product {SerialNumber} unregistered from consumer {ConsumerId} by {Role} {UserId}",
                product.SerialNumber, removedOwnerId, role, userId);

            // ===== NOTIFICATION =====
            await _notificationService.CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = removedOwnerId,
                RecipientType = "consumer",
                Type = NotificationType.Warning,
                Title = "Product Unregistered",
                Message = $"Product {product.SerialNumber} (Model: {product.Model}) has been unregistered from your account. You will no longer receive notifications for this product.",
                RelatedEntityId = product.Id,
                RelatedEntityType = "product",
                Priority = NotificationPriority.High
            });
        }

        private async Task HandleSelfUnregisterAsync(Product product, Guid userId, string role)
        {
            // Only consumers can self-unregister
            if (role != RoleConsumer)
            {
                _logger.LogWarning("Non-consumer {Role} {UserId} attempted to self-unregister from product {SerialNumber}",
                    role, userId, product.SerialNumber);
                throw new UnauthorizedAccessException("Only consumers can unregister themselves from products");
            }

            // Check if consumer owns this product
            if (product.OwnerId != userId)
            {
                _logger.LogWarning("Consumer {UserId} attempted to unregister from product {SerialNumber} they don't own",
                    userId, product.SerialNumber);
                throw new UnauthorizedAccessException("You can only unregister from products you own");
            }

            // Check if consumer is actually assigned
            if (!product.OwnerId.HasValue)
            {
                _logger.LogWarning("Consumer {UserId} attempted to unregister from product {SerialNumber} that has no owner",
                    userId, product.SerialNumber);
                throw new InvalidOperationException("Product is not registered to any consumer");
            }

            product.OwnerId = null;
            product.RegisteredAt = DateTime.UtcNow;

            _logger.LogInformation("Consumer {ConsumerId} self-unregistered from product {SerialNumber}",
                userId, product.SerialNumber);

            // ===== NOTIFICATIONS =====
            var notifications = new List<NotificationCreateDTO>
            {
                // Notify consumer (confirmation)
                new NotificationCreateDTO
                {
                    RecipientId = userId,
                    RecipientType = "consumer",
                    Type = NotificationType.Info,
                    Title = "Product Unregistered",
                    Message = $"You have successfully unregistered product {product.SerialNumber} (Model: {product.Model}). You will no longer receive notifications for this product.",
                    RelatedEntityId = product.Id,
                    RelatedEntityType = "product",
                    Priority = NotificationPriority.Normal
                }
            };

            // Notify reseller if assigned
            if (product.ResellerId.HasValue)
            {
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = product.ResellerId.Value,
                    RecipientType = "reseller",
                    Type = NotificationType.Info,
                    Title = "Consumer Unregistered Product",
                    Message = $"A consumer has unregistered product {product.SerialNumber} (Model: {product.Model}).",
                    RelatedEntityId = product.Id,
                    RelatedEntityType = "product",
                    Priority = NotificationPriority.Low
                });
            }

            // Notify manufacturer
            notifications.Add(new NotificationCreateDTO
            {
                RecipientId = product.ManufacturerId,
                RecipientType = "manufacturer",
                Type = NotificationType.Info,
                Title = "Consumer Unregistered Product",
                Message = $"A consumer has unregistered product {product.SerialNumber} (Model: {product.Model}).",
                RelatedEntityId = product.Id,
                RelatedEntityType = "product",
                Priority = NotificationPriority.Low
            });

            await _notificationService.CreateBulkNotificationsAsync(notifications);
        }

        public async Task<ProductResponseDTO> GetProductBySerialNumberAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                throw new ArgumentException("Serial number cannot be empty");

            var product = await _context.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Reseller)
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.SerialNumber == serialNumber.Trim());

            if (product == null)
            {
                _logger.LogWarning("Product not found: {SerialNumber}", serialNumber);
                throw new KeyNotFoundException($"Product with serial number {serialNumber} not found");
            }

            return MapToResponseDTO(product);
        }

        public async Task<Product> UpdateProductAsync(string serialNumber, ProductUpdateDTO dto, Guid userId, string role)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                throw new ArgumentException("Serial number cannot be empty");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SerialNumber == serialNumber.Trim())
                ?? throw new KeyNotFoundException($"Product with serial number {serialNumber} not found");

            // Authorization checks
            if (role == RoleManufacturer && product.ManufacturerId != userId)
            {
                _logger.LogWarning("Manufacturer {ManufacturerId} attempted to update product {SerialNumber} owned by another manufacturer", 
                    userId, serialNumber);
                throw new UnauthorizedAccessException("You can only update your own products");
            }

            if (role == RoleReseller && product.ResellerId != userId)
            {
                _logger.LogWarning("Reseller {ResellerId} attempted to update product {SerialNumber} not assigned to them", 
                    userId, serialNumber);
                throw new UnauthorizedAccessException("You can only update products assigned to you");
            }

            // Update allowed fields
            product.Model = dto.Model.Trim();

            var notifications = new List<NotificationCreateDTO>();

            // Manufacturers can assign to resellers
            if (role == RoleManufacturer && dto.ResellerId.HasValue)
            {
                // Validate reseller exists
                var resellerExists = await _context.Resellers.AnyAsync(r => r.Id == dto.ResellerId.Value && !r.IsDeleted);
                if (!resellerExists)
                {
                    _logger.LogWarning("Attempted to assign product {SerialNumber} to non-existent reseller {ResellerId}", 
                        serialNumber, dto.ResellerId);
                    throw new ArgumentException($"Reseller with ID {dto.ResellerId} not found");
                }

                product.ResellerId = dto.ResellerId;
                _logger.LogInformation("Product {SerialNumber} assigned to reseller {ResellerId}", 
                    serialNumber, dto.ResellerId);

                // Notify reseller
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = dto.ResellerId.Value,
                    RecipientType = "reseller",
                    Type = NotificationType.ProductRegistered,
                    Title = "Product Assigned to Your Inventory",
                    Message = $"Product {product.SerialNumber} (Model: {product.Model}) has been assigned to you for distribution.",
                    RelatedEntityId = product.Id,
                    RelatedEntityType = "product",
                    Priority = NotificationPriority.Normal
                });
            }

            // Resellers can assign to consumers
            if (role == RoleReseller && dto.OwnerId.HasValue)
            {
                if (product.OwnerId.HasValue)
                {
                    _logger.LogWarning("Attempted to reassign product {SerialNumber} that already has owner", serialNumber);
                    throw new InvalidOperationException("Product already has an owner");
                }

                // Validate consumer exists
                var consumerExists = await _context.Consumers.AnyAsync(c => c.Id == dto.OwnerId.Value && !c.IsDeleted);
                if (!consumerExists)
                {
                    _logger.LogWarning("Attempted to assign product {SerialNumber} to non-existent consumer {OwnerId}", 
                        serialNumber, dto.OwnerId);
                    throw new ArgumentException($"Consumer with ID {dto.OwnerId} not found");
                }

                product.OwnerId = dto.OwnerId;
                _logger.LogInformation("Product {SerialNumber} assigned to consumer {OwnerId}", 
                    serialNumber, dto.OwnerId);

                // Notify consumer
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = dto.OwnerId.Value,
                    RecipientType = "consumer",
                    Type = NotificationType.ProductRegistered,
                    Title = "Product Registered Successfully",
                    Message = $"Product {product.SerialNumber} (Model: {product.Model}) has been registered to your account. You will receive notifications about any recalls affecting this product.",
                    RelatedEntityId = product.Id,
                    RelatedEntityType = "product",
                    Priority = NotificationPriority.Normal
                });

                // Notify manufacturer
                notifications.Add(new NotificationCreateDTO
                {
                    RecipientId = product.ManufacturerId,
                    RecipientType = "manufacturer",
                    Type = NotificationType.Info,
                    Title = "Product Sold to Consumer",
                    Message = $"Product {product.SerialNumber} (Model: {product.Model}) has been sold to a consumer.",
                    RelatedEntityId = product.Id,
                    RelatedEntityType = "product",
                    Priority = NotificationPriority.Low
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {SerialNumber} updated by {Role} {UserId}", 
                serialNumber, role, userId);

            // Send notifications if any
            if (notifications.Count > 0)
            {
                await _notificationService.CreateBulkNotificationsAsync(notifications);
            }

            return product;
        }

        public async Task<bool> DeleteProductAsync(string serialNumber, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                _logger.LogWarning("Delete attempt with empty serial number by user {UserId}", userId);
                throw new ArgumentException("Serial number cannot be empty");
            }

            // Use IgnoreQueryFilters to find even soft-deleted products
            var product = await _context.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.SerialNumber == serialNumber.Trim());

            if (product == null)
            {
                _logger.LogWarning("User {UserId} attempted to delete non-existent product: {SerialNumber}", 
                    userId, serialNumber);
                throw new KeyNotFoundException($"Product with serial number {serialNumber} not found");
            }

            // Check if already deleted
            if (product.IsDeleted)
            {
                _logger.LogWarning("User {UserId} attempted to delete already deleted product: {SerialNumber}", 
                    userId, serialNumber);
                throw new InvalidOperationException($"Product {serialNumber} is already deleted");
            }

            // Authorization: Only the manufacturer who created the product can delete it
            if (product.ManufacturerId != userId)
            {
                _logger.LogWarning("Manufacturer {UserId} attempted to delete product {SerialNumber} owned by manufacturer {ManufacturerId}", 
                    userId, serialNumber, product.ManufacturerId);
                throw new UnauthorizedAccessException("You can only delete products you created");
            }

            // Check if product is assigned to reseller or consumer
            if (product.ResellerId.HasValue || product.OwnerId.HasValue)
            {
                _logger.LogWarning("Manufacturer {UserId} attempted to delete product {SerialNumber} that is already in distribution chain", 
                    userId, serialNumber);
                throw new InvalidOperationException("Cannot delete product that has been assigned to resellers or consumers. Contact support for assistance.");
            }

            // Perform soft delete
            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
            product.DeletedBy = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {SerialNumber} soft deleted by manufacturer {UserId} at {DeletedAt}", 
                serialNumber, userId, product.DeletedAt);

            // ===== NOTIFICATION =====
            await _notificationService.CreateNotificationAsync(new NotificationCreateDTO
            {
                RecipientId = userId,
                RecipientType = "manufacturer",
                Type = NotificationType.Info,
                Title = "Product Deleted",
                Message = $"Product {product.SerialNumber} (Model: {product.Model}) has been successfully deleted from your inventory.",
                RelatedEntityId = product.Id,
                RelatedEntityType = "product",
                Priority = NotificationPriority.Normal
            });

            return true;
        }

        // List methods - Updated to return ProductResponseDTO
        public async Task<PagedResultsDTO<ProductResponseDTO>> GetManufacturerProductsAsync(Guid manufacturerId, int page, int pageSize)
        {
            if (page < 1)
            {
                _logger.LogWarning("Invalid page number {Page} requested, defaulting to 1", page);
                page = 1;
            }

            if (pageSize < 1 || pageSize > 100)
            {
                _logger.LogWarning("Invalid page size {PageSize} requested, defaulting to 20", pageSize);
                pageSize = 20;
            }

            var query = _context.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Reseller)
                .Include(p => p.Owner)
                .Where(p => p.ManufacturerId == manufacturerId)
                .OrderByDescending(p => p.RegisteredAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} products for manufacturer {ManufacturerId} (Page {Page} of {TotalPages})",
                items.Count, manufacturerId, page, Math.Ceiling(totalCount / (double)pageSize));

            return new PagedResultsDTO<ProductResponseDTO>
            {
                Items = [.. items.Select(MapToResponseDTO)],
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultsDTO<ProductResponseDTO>> GetResellerProductsAsync(Guid resellerId, int page, int pageSize)
        {
            if (page < 1)
            {
                _logger.LogWarning("Invalid page number {Page} requested, defaulting to 1", page);
                page = 1;
            }

            if (pageSize < 1 || pageSize > 100)
            {
                _logger.LogWarning("Invalid page size {PageSize} requested, defaulting to 20", pageSize);
                pageSize = 20;
            }

            var query = _context.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Reseller)
                .Include(p => p.Owner)
                .Where(p => p.ResellerId == resellerId)
                .OrderByDescending(p => p.RegisteredAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} products for reseller {ResellerId} (Page {Page} of {TotalPages})",
                items.Count, resellerId, page, Math.Ceiling(totalCount / (double)pageSize));

            return new PagedResultsDTO<ProductResponseDTO>
            {
                Items = [.. items.Select(MapToResponseDTO)],
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultsDTO<ProductResponseDTO>> GetConsumerProductsAsync(Guid consumerId, int page, int pageSize)
        {
            if (page < 1)
            {
                _logger.LogWarning("Invalid page number {Page} requested, defaulting to 1", page);
                page = 1;
            }

            if (pageSize < 1 || pageSize > 100)
            {
                _logger.LogWarning("Invalid page size {PageSize} requested, defaulting to 20", pageSize);
                pageSize = 20;
            }

            var query = _context.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Reseller)
                .Include(p => p.Owner)
                .Where(p => p.OwnerId == consumerId)
                .OrderByDescending(p => p.RegisteredAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} products for consumer {ConsumerId} (Page {Page} of {TotalPages})",
                items.Count, consumerId, page, Math.Ceiling(totalCount / (double)pageSize));

            return new PagedResultsDTO<ProductResponseDTO>
            {
                Items = [.. items.Select(MapToResponseDTO)],
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // Helper method to map Product to ProductResponseDTO
        private ProductResponseDTO MapToResponseDTO(Product product)
        {
            return new ProductResponseDTO
            {
                Id = product.Id,
                SerialNumber = product.SerialNumber,
                Model = product.Model,
                RegisteredAt = product.RegisteredAt,
                ManufacturerId = product.ManufacturerId,
                Manufacturer = product.Manufacturer != null ? new ManufacturerBasicDTO
                {
                    Id = product.Manufacturer.Id,
                    CompanyName = product.Manufacturer.CompanyName,
                    Email = product.Manufacturer.Email,
                    WalletAddress = product.Manufacturer.WalletAddress
                } : null,
                ResellerId = product.ResellerId,
                Reseller = product.Reseller != null ? new ResellerBasicDTO
                {
                    Id = product.Reseller.Id,
                    CompanyName = product.Reseller.CompanyName,
                    Email = product.Reseller.Email,
                    WalletAddress = product.Reseller.WalletAddress
                } : null,
                OwnerId = product.OwnerId,
                Owner = product.Owner != null ? new ConsumerBasicDTO
                {
                    Id = product.Owner.Id,
                    Name = product.Owner.Name,
                    Email = product.Owner.Email ?? string.Empty,
                    PhoneNumber = product.Owner.PhoneNumber,
                    WalletAddress = product.Owner.WalletAddress
                } : null
            };
        }
    }
}