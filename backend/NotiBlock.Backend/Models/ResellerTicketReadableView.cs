namespace NotiBlock.Backend.Models
{
    public class ResellerTicketReadableView
    {
        public Guid Id { get; set; }
        public Guid ResellerId { get; set; }
        
        // Category
        public string CategoryText { get; set; } = string.Empty;
        public int CategoryCode { get; set; }
        
        public string Description { get; set; } = string.Empty;
        
        // Status
        public string StatusText { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        
        // Priority
        public string PriorityText { get; set; } = string.Empty;
        public int PriorityCode { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public Guid? ApprovedById { get; set; }
        public Guid? ResolvedBy { get; set; }
        public string? ResolutionNotes { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
