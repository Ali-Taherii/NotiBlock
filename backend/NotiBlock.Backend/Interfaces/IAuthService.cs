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

        // Common Auth Methods
        Task ChangePasswordAsync(Guid userId, string role, ChangePasswordDTO dto);
        //Task RequestPasswordResetAsync(string email, string userType);
        //Task ResetPasswordAsync(ResetPasswordDTO dto);
        Task<object> UpdateProfileAsync(Guid userId, string role, UpdateProfileDTO dto);

        Task DeleteAccountAsync(Guid userId, string role, string password);
        //Task VerifyEmailAsync(string token);
        Task<bool> IsEmailAvailableAsync(string email, string userType);
        Task<object> GetUserStatsAsync(Guid userId, string role);

    }
}