using NotiBlock.Backend.Models;
using NotiBlock.Backend.DTOs;

namespace NotiBlock.Backend.Interfaces
{

    public interface IAuthService
    {
        Task<AppUser> RegisterAsync(AuthDTO.AuthRegisterDto dto);
        Task<string> LoginAsync(AuthDTO.AuthLoginDto dto);
    }
}