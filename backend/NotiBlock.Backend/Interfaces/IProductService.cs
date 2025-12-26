using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(ProductCreateDTO dto, Guid manufacturerId);
        Task<Product> RegisterProductAsync(ProductRegisterDTO dto, Guid registererId, string role);
        Task<Product> GetProductBySerialNumberAsync(string serialNumber);
        Task<Product> UpdateProductAsync(string serialNumber, ProductUpdateDTO dto, Guid userId, string role);
        Task<bool> DeleteProductAsync(string serialNumber);
    }
}
