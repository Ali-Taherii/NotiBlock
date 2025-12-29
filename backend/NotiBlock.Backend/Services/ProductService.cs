using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services
{
    public class ProductService(AppDbContext context, ILogger<ProductService> logger) : IProductService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<ProductService> _logger = logger;

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

                product.OwnerId = registererId;
                product.RegisteredAt = DateTime.UtcNow;
                _logger.LogInformation("Product {SerialNumber} registered to consumer {ConsumerId}", 
                    dto.SerialNumber, registererId);
            }
            else if (role == RoleReseller)
            {
                if (product.ResellerId.HasValue)
                {
                    _logger.LogWarning("Attempted to register already assigned product {SerialNumber} to reseller {ResellerId}", 
                        dto.SerialNumber, registererId);
                    throw new InvalidOperationException("Product already registered to a reseller");
                }

                product.ResellerId = registererId;
                product.RegisteredAt = DateTime.UtcNow;
                _logger.LogInformation("Product {SerialNumber} registered to reseller {ResellerId}", 
                    dto.SerialNumber, registererId);
            }
            else
            {
                _logger.LogWarning("Invalid role {Role} attempted to register product {SerialNumber}", 
                    role, dto.SerialNumber);
                throw new InvalidOperationException($"Invalid role '{role}' for registration");
            }

            await _context.SaveChangesAsync();
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
                    // Only manufacturers can remove reseller assignments
                    if (role != RoleManufacturer)
                    {
                        _logger.LogWarning("Non-manufacturer {Role} {UserId} attempted to remove reseller from product {SerialNumber}",
                            role, userId, dto.SerialNumber);
                        throw new UnauthorizedAccessException("Only manufacturers can remove reseller assignments");
                    }

                    // Verify the manufacturer owns this product
                    if (product.ManufacturerId != userId)
                    {
                        _logger.LogWarning("Manufacturer {UserId} attempted to unregister product {SerialNumber} owned by manufacturer {ManufacturerId}",
                            userId, dto.SerialNumber, product.ManufacturerId);
                        throw new UnauthorizedAccessException("You can only unregister products you created");
                    }

                    // Check if reseller is assigned
                    if (!product.ResellerId.HasValue)
                    {
                        _logger.LogWarning("Attempted to remove reseller from product {SerialNumber} that has no reseller assigned",
                            dto.SerialNumber);
                        throw new InvalidOperationException("Product does not have a reseller assigned");
                    }

                    // Check if product has been sold to consumer
                    if (product.OwnerId.HasValue)
                    {
                        _logger.LogWarning("Attempted to remove reseller from product {SerialNumber} that has already been sold to consumer",
                            dto.SerialNumber);
                        throw new InvalidOperationException("Cannot remove reseller from product that has been sold to a consumer");
                    }

                    var removedResellerId = product.ResellerId;
                    product.ResellerId = null;
                    product.RegisteredAt = DateTime.UtcNow;

                    _logger.LogInformation("Product {SerialNumber} unregistered from reseller {ResellerId} by manufacturer {ManufacturerId}",
                        dto.SerialNumber, removedResellerId, userId);
                    break;

                case UnregisterType.RemoveConsumer:
                    // Both manufacturers and resellers can remove consumer assignments
                    if (role == RoleManufacturer)
                    {
                        // Verify the manufacturer owns this product
                        if (product.ManufacturerId != userId)
                        {
                            _logger.LogWarning("Manufacturer {UserId} attempted to unregister consumer from product {SerialNumber} owned by manufacturer {ManufacturerId}",
                                userId, dto.SerialNumber, product.ManufacturerId);
                            throw new UnauthorizedAccessException("You can only unregister products you created");
                        }
                    }
                    else if (role == RoleReseller)
                    {
                        // Verify the reseller is assigned to this product
                        if (product.ResellerId != userId)
                        {
                            _logger.LogWarning("Reseller {UserId} attempted to unregister consumer from product {SerialNumber} assigned to reseller {ResellerId}",
                                userId, dto.SerialNumber, product.ResellerId);
                            throw new UnauthorizedAccessException("You can only unregister products assigned to you");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid role {Role} attempted to remove consumer from product {SerialNumber}",
                            role, dto.SerialNumber);
                        throw new UnauthorizedAccessException("Only manufacturers and resellers can remove consumer assignments");
                    }

                    // Check if consumer is assigned
                    if (!product.OwnerId.HasValue)
                    {
                        _logger.LogWarning("Attempted to remove consumer from product {SerialNumber} that has no consumer assigned",
                            dto.SerialNumber);
                        throw new InvalidOperationException("Product does not have a consumer assigned");
                    }

                    var removedOwnerId = product.OwnerId;
                    product.OwnerId = null;
                    product.RegisteredAt = DateTime.UtcNow;

                    _logger.LogInformation("Product {SerialNumber} unregistered from consumer {ConsumerId} by {Role} {UserId}",
                        dto.SerialNumber, removedOwnerId, role, userId);
                    break;

                default:
                    throw new ArgumentException($"Invalid unregister type: {dto.Type}");
            }

            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> GetProductBySerialNumberAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                throw new ArgumentException("Serial number cannot be empty");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SerialNumber == serialNumber.Trim());

            if (product == null)
            {
                _logger.LogWarning("Product not found: {SerialNumber}", serialNumber);
                throw new KeyNotFoundException($"Product with serial number {serialNumber} not found");
            }

            return product;
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

            // Manufacturers can assign to resellers
            if (role == RoleManufacturer && dto.ResellerId.HasValue)
            {
                // Validate reseller exists
                var resellerExists = await _context.Resellers.AnyAsync(r => r.Id == dto.ResellerId.Value);
                if (!resellerExists)
                {
                    _logger.LogWarning("Attempted to assign product {SerialNumber} to non-existent reseller {ResellerId}", 
                        serialNumber, dto.ResellerId);
                    throw new ArgumentException($"Reseller with ID {dto.ResellerId} not found");
                }

                product.ResellerId = dto.ResellerId;
                _logger.LogInformation("Product {SerialNumber} assigned to reseller {ResellerId}", 
                    serialNumber, dto.ResellerId);
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
                var consumerExists = await _context.Consumers.AnyAsync(c => c.Id == dto.OwnerId.Value);
                if (!consumerExists)
                {
                    _logger.LogWarning("Attempted to assign product {SerialNumber} to non-existent consumer {OwnerId}", 
                        serialNumber, dto.OwnerId);
                    throw new ArgumentException($"Consumer with ID {dto.OwnerId} not found");
                }

                product.OwnerId = dto.OwnerId;
                _logger.LogInformation("Product {SerialNumber} assigned to consumer {OwnerId}", 
                    serialNumber, dto.OwnerId);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {SerialNumber} updated by {Role} {UserId}", 
                serialNumber, role, userId);

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

            return true;
        }

        // List methods
        public async Task<PagedResultsDTO<Product>> GetManufacturerProductsAsync(Guid manufacturerId, int page, int pageSize)
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
                .Where(p => p.ManufacturerId == manufacturerId)
                .OrderByDescending(p => p.RegisteredAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} products for manufacturer {ManufacturerId} (Page {Page} of {TotalPages})",
                items.Count, manufacturerId, page, Math.Ceiling(totalCount / (double)pageSize));

            return new PagedResultsDTO<Product>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultsDTO<Product>> GetResellerProductsAsync(Guid resellerId, int page, int pageSize)
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
                .Where(p => p.ResellerId == resellerId)
                .OrderByDescending(p => p.RegisteredAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} products for reseller {ResellerId} (Page {Page} of {TotalPages})",
                items.Count, resellerId, page, Math.Ceiling(totalCount / (double)pageSize));

            return new PagedResultsDTO<Product>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultsDTO<Product>> GetConsumerProductsAsync(Guid consumerId, int page, int pageSize)
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
                .Where(p => p.OwnerId == consumerId)
                .OrderByDescending(p => p.RegisteredAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} products for consumer {ConsumerId} (Page {Page} of {TotalPages})",
                items.Count, consumerId, page, Math.Ceiling(totalCount / (double)pageSize));

            return new PagedResultsDTO<Product>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}