using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services
{
    public class ProductService(AppDbContext context) : IProductService
    {
        private readonly AppDbContext _context = context;

        public async Task<Product> RegisterProductAsync(ProductRegisterDTO dto, Guid consumerId)
        {
            var exists = await _context.Products.AnyAsync(p => p.SerialNumber == dto.SerialNumber);
            if (exists) throw new Exception("Product already registered");

            var product = new Product
            {
                SerialNumber = dto.SerialNumber,
                Model = dto.Model,
                ManufacturerId = dto.ManufacturerId,
                ResellerId = dto.ResellerId,
                OwnerId = consumerId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
    }
}
