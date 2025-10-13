using System.Text.Json.Serialization;

namespace NotiBlock.Backend.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        
        public required string Role { get; set; }
    }
}