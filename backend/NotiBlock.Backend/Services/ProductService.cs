using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Extensions;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services
{
    public class ProductService(AppDbContext context, ILogger<ProductService> logger) : IProductService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<ProductService> _logger = logger;

        public async Task<Product> CreateProductAsync(ProductCreateDTO dto, Guid manufacturerId)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.SerialNumber))
                throw new ArgumentException("Serial number cannot be empty");

            if (string.IsNullOrWhiteSpace(dto.Model))
                throw new ArgumentException("Model cannot be empty");

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

        public async Task<Product> RegisterProductAsync(ProductRegisterDTO dto, Guid registererId, string roleString)
        {
            if (string.IsNullOrWhiteSpace(dto.SerialNumber))
                throw new ArgumentException("Serial number cannot be empty");

            // Parse role string to enum
            if (!roleString.TryParseUserRole(out var role))
                throw new ArgumentException($"Invalid role: {roleString}");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SerialNumber == dto.SerialNumber.Trim())
                ?? throw new KeyNotFoundException($"Product with serial number {dto.SerialNumber} not found");

            switch (role)
            {
                case UserRole.Consumer:
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
                    break;

                case UserRole.Reseller:
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
                    break;

                default:
                    _logger.LogWarning("Invalid role {Role} attempted to register product {SerialNumber}",
                        role, dto.SerialNumber);
                    throw new InvalidOperationException($"Role '{role}' cannot register products");
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

        public async Task<Product> UpdateProductAsync(string serialNumber, ProductUpdateDTO dto, Guid userId, string roleString)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                throw new ArgumentException("Serial number cannot be empty");

            if (string.IsNullOrWhiteSpace(dto.Model))
                throw new ArgumentException("Model cannot be empty");

            // Parse role string to enum
            if (!roleString.TryParseUserRole(out var role))
                throw new ArgumentException($"Invalid role: {roleString}");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SerialNumber == serialNumber.Trim())
                ?? throw new KeyNotFoundException($"Product with serial number {serialNumber} not found");

            // Authorization checks
            switch (role)
            {
                case UserRole.Manufacturer:
                    if (product.ManufacturerId != userId)
                    {
                        _logger.LogWarning("Manufacturer {ManufacturerId} attempted to update product {SerialNumber} owned by another manufacturer",
                            userId, serialNumber);
                        throw new UnauthorizedAccessException("You can only update your own products");
                    }

                    // Update allowed fields
                    product.Model = dto.Model.Trim();

                    // Manufacturers can assign to resellers
                    if (dto.ResellerId.HasValue)
                    {
                        product.ResellerId = dto.ResellerId;
                        _logger.LogInformation("Product {SerialNumber} assigned to reseller {ResellerId}",
                            serialNumber, dto.ResellerId);
                    }
                    break;

                case UserRole.Reseller:
                    if (product.ResellerId != userId)
                    {
                        _logger.LogWarning("Reseller {ResellerId} attempted to update product {SerialNumber} not assigned to them",
                            userId, serialNumber);
                        throw new UnauthorizedAccessException("You can only update products assigned to you");
                    }

                    // Update allowed fields
                    product.Model = dto.Model.Trim();

                    // Resellers can assign to consumers
                    if (dto.OwnerId.HasValue)
                    {
                        if (product.OwnerId.HasValue)
                        {
                            _logger.LogWarning("Attempted to reassign product {SerialNumber} that already has owner", serialNumber);
                            throw new InvalidOperationException("Product already has an owner");
                        }
                        product.OwnerId = dto.OwnerId;
                        _logger.LogInformation("Product {SerialNumber} assigned to consumer {OwnerId}",
                            serialNumber, dto.OwnerId);
                    }
                    break;

                default:
                    _logger.LogWarning("Role {Role} attempted to update product {SerialNumber}",
                        role, serialNumber);
                    throw new UnauthorizedAccessException($"Role '{role}' cannot update products");
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {SerialNumber} updated by {Role} {UserId}",
                serialNumber, role, userId);

            return product;
        }

        public async Task<bool> DeleteProductAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                throw new ArgumentException("Serial number cannot be empty");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SerialNumber == serialNumber.Trim());

            if (product == null)
            {
                _logger.LogWarning("Attempted to delete non-existent product: {SerialNumber}", serialNumber);
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product deleted: {SerialNumber}", serialNumber);
            return true;
        }
    }
}