namespace NotiBlock.Backend.DTOs
{
    public class ConsumerReportDTO
    {
        public Guid ProductId { get; set; }
        public string IssueDescription { get; set; } = string.Empty;
    }
}
