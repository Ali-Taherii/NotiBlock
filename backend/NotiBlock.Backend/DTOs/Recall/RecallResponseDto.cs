using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.DTOs
{
    public class RecallResponseDTO
    {
        public Guid Id { get; set; }
        public Guid ManufacturerId { get; set; }
        public string ManufacturerName { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;
        public RecallStatus Status { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? TransactionHash { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RegulatorNotes { get; set; }
        public int PendingUpdateRequestCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
