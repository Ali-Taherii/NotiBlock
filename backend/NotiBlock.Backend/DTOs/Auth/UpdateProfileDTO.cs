namespace NotiBlock.Backend.DTOs.Auth
{
    public class UpdateProfileDTO
    {
        public string? Name { get; set; } // Consumer name or Company/Agency name
        public string? PhoneNumber { get; set; }
        public string? WalletAddress { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
    }
}