namespace NotiBlock.Backend.Models
{
    public class BlockchainRecall
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Recall reference
        public Guid RecallId { get; set; }
        public Recall? Recall { get; set; }

        // On-chain data
        public string TransactionHash { get; set; } = string.Empty;
        public ulong? BlockNumber { get; set; }
        public int ChainId { get; set; } = 11155111; // Sepolia
        
        // Metadata hash (IPFS or computed hash)
        public string MetadataHash { get; set; } = string.Empty;

        // Event tracking
        public string? EventSignature { get; set; } // RecallIssued or RecallStatusChanged
        public DateTime TransactionConfirmedAt { get; set; } = DateTime.UtcNow;
        public int ConfirmationCount { get; set; } = 0;

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }

    public class RecallBlockchainEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Recall reference
        public Guid RecallId { get; set; }
        public Recall? Recall { get; set; }

        // Event details
        public string EventType { get; set; } = string.Empty; // RecallIssued or RecallStatusChanged
        public string TransactionHash { get; set; } = string.Empty;
        public ulong? BlockNumber { get; set; }
        public int ChainId { get; set; } = 11155111;

        // Event parameters
        public string MetadataHash { get; set; } = string.Empty;
        public string ActorRole { get; set; } = string.Empty; // manufacturer, regulator
        public string Actor { get; set; } = string.Empty; // User address or ID
        public string? NewStatus { get; set; } // For RecallStatusChanged

        // Timestamps
        public DateTime EventEmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
