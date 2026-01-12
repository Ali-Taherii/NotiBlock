using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(ProductCreateDTO dto, Guid manufacturerId);
        Task<Product> RegisterProductAsync(ProductRegisterDTO dto, Guid registererId, string role);
        Task<Product> UpdateProductAsync(string serialNumber, ProductUpdateDTO dto, Guid userId, string role);
        Task<bool> DeleteProductAsync(string serialNumber, Guid userId);
        
        // List endpoints
        Task<ProductResponseDTO> GetProductBySerialNumberAsync(string serialNumber);
        Task<PagedResultsDTO<ProductResponseDTO>> GetManufacturerProductsAsync(Guid manufacturerId, int page, int pageSize);
        Task<PagedResultsDTO<ProductResponseDTO>> GetResellerProductsAsync(Guid resellerId, int page, int pageSize);
        Task<PagedResultsDTO<ProductResponseDTO>> GetConsumerProductsAsync(Guid consumerId, int page, int pageSize);
        
        // New unregister endpoint
        Task<Product> UnregisterProductAsync(ProductUnregisterDTO dto, Guid userId, string role);
    }
}
