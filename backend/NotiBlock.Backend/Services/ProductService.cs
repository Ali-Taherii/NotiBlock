using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services
{
    public class ProductService(AppDbContext context) :  IProductService
    {
        private readonly AppDbContext _context = context;

        public async Task<Product> CreateProductAsync(ProductCreateDTO dto, Guid manufacturerId)
        {
            var product = new Product
            {
                SerialNumber = dto.SerialNumber,
                Model = dto.Model,
                ManufacturerId = manufacturerId,
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> RegisterProductAsync(ProductRegisterDTO dto, Guid registererId, string role)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SerialNumber == dto.SerialNumber) 
                ?? throw new KeyNotFoundException("Product not found");
            
            if (role == "consumer")
            {
                if (product.OwnerId.HasValue)
                    throw new InvalidOperationException("Product already registered to a consumer");
                
                product.OwnerId = registererId;
            }
            else if (role == "reseller")
            {
                if (product.ResellerId.HasValue)
                    throw new InvalidOperationException("Product already registered to a reseller");
                
                product.ResellerId = registererId;
            }
            else
            {
                throw new InvalidOperationException("Invalid role for registration");
            }
            
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> GetProductBySerialNumberAsync(string serialNumber)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.SerialNumber == serialNumber);
            return product ?? throw new InvalidOperationException("Product not found");
        }

        public async Task<Product> UpdateProductAsync(string serialNumber, ProductUpdateDTO dto, Guid userId, string role)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.SerialNumber == serialNumber) 
                ?? throw new KeyNotFoundException("Product not found");
            
            // Authorization: Only manufacturer who created it can update
            if (role == "manufacturer" && product.ManufacturerId != userId)
                throw new UnauthorizedAccessException("You can only update your own products");
            
            // Authorization: Only reseller who owns it can update
            if (role == "reseller" && product.ResellerId != userId)
                throw new UnauthorizedAccessException("You can only update products you own");
            
            // DO NOT allow changing serial number or manufacturer
            // Only update allowed fields
            product.Model = dto.Model;
            
            if (role == "manufacturer" && dto.ResellerId.HasValue)
                product.ResellerId = dto.ResellerId;
            
            if (role == "reseller" && dto.OwnerId.HasValue)
                product.OwnerId = dto.OwnerId;

            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProductAsync(string serialNumber)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.SerialNumber == serialNumber);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}