using NotiBlock.Backend.Models;
using NotiBlock.Backend.DTOs;

namespace NotiBlock.Backend.Interfaces
{

    public interface IAuthService
    {
        // Consumer Auth
        Task<string> RegisterConsumerAsync(AuthRegisterDTO dto);
        Task<string> LoginConsumerAsync(AuthLoginDTO dto);


        // Reseller Auth
        Task<string> RegisterResellerAsync(AuthRegisterDTO dto);
        Task<string> LoginResellerAsync(AuthLoginDTO dto);


        // Manufacturer Auth
        Task<string> RegisterManufacturerAsync(AuthRegisterDTO dto);
        Task<string> LoginManufacturerAsync(AuthLoginDTO dto);


        // Regulator Auth
        Task<string> RegisterRegulatorAsync(AuthRegisterDTO dto);
        Task<string> LoginRegulatorAsync(AuthLoginDTO dto);

    }
}