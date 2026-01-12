using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.DTOs.Auth
{
    public class ForgotPasswordDTO
    {
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}