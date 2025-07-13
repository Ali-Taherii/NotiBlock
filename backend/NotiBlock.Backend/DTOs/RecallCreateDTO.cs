namespace NotiBlock.Backend.DTOs
{
    public class RecallCreateDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;
    }
}