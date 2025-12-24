using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface IProductService
    {
        Task<Product> RegisterProductAsync(ProductRegisterDTO dto, Guid consumerId);
    }
}
