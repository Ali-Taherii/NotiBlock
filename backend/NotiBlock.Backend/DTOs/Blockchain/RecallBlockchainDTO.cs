namespace NotiBlock.Backend.DTOs
{
    public class RecallBlockchainDTO
    {
        public Guid RecallId { get; set; }
        public string TransactionHash { get; set; } = string.Empty;
        public ulong? BlockNumber { get; set; }
        public int ChainId { get; set; }
        public string MetadataHash { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime TransactionConfirmedAt { get; set; }
        public int ConfirmationCount { get; set; }
    }

    public class RecallIssuedEventDTO
    {
        public Guid RecallId { get; set; }
        public string ProductHash { get; set; } = string.Empty;
        public string MetadataHash { get; set; } = string.Empty;
        public string ActorRole { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;
    }

    public class RecallStatusChangedEventDTO
    {
        public Guid RecallId { get; set; }
        public string MetadataHash { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public string ActorRole { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;
    }

    public class MetadataDTO
    {
        public Guid RecallId { get; set; }
        public string ProductSerialNumber { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public string ManufacturerId { get; set; } = string.Empty;
    }
}
