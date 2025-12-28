using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace NotiBlock.Backend.Models
{
    public class Consumer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;

        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string WalletAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

    }
}