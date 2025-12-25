using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(ProductCreateDTO dto, Guid manufacturerId);
        Task<Product> RegisterProductAsync(ProductRegisterDTO dto, Guid registererId);
        Task<Product> GetProductBySerialNumberAsync(string serialNumber);
        Task<Product> UpdateProductAsync(string serialNumber, ProductRegisterDTO dto);
        Task<bool> DeleteProductAsync(string serialNumber);
    }
}
