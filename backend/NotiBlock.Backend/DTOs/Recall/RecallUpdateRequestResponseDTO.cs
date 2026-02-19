using System;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.DTOs
{
    public class RecallUpdateRequestResponseDTO
    {
        public Guid Id { get; set; }
        public Guid RecallId { get; set; }
        public Guid ManufacturerId { get; set; }
        public string ManufacturerName { get; set; } = string.Empty;
        public string? ProposedReason { get; set; }
        public string? ProposedActionRequired { get; set; }
        public RecallStatus? ProposedStatus { get; set; }
        public string? ManufacturerNotes { get; set; }
        public RecallUpdateRequestStatus Status { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RegulatorNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
