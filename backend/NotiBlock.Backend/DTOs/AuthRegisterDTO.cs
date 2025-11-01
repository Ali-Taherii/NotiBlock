namespace NotiBlock.Backend.DTOs
{
    public class AuthRegisterDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty; // for Consumer
        public string Name { get; set; } = string.Empty; // or CompanyName/AgencyName
        public string WalletAddress { get; set; } = string.Empty;
    }
}
