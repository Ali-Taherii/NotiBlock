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

        public async Task<Product> RegisterProductAsync(ProductRegisterDTO dto, Guid registererId)
        {
            var exists = await _context.Products.AnyAsync(p => p.SerialNumber == dto.SerialNumber);
            if (exists) throw new Exception("Product already registered");

            var product = new Product
            {
                SerialNumber = dto.SerialNumber,
                ManufacturerId = dto.ManufacturerId,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> GetProductBySerialNumberAsync(string serialNumber)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.SerialNumber == serialNumber);
            return product ?? throw new InvalidOperationException("Product not found");
        }

        public async Task<Product> UpdateProductAsync(string serialNumber, ProductRegisterDTO dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.SerialNumber == serialNumber) ?? throw new InvalidOperationException("Product not found");
            product.SerialNumber = dto.SerialNumber;
            product.ManufacturerId = dto.ManufacturerId;

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