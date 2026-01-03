namespace NotiBlock.Backend.DTOs
{
    public class NotificationMarkAsReadDTO
    {
        public List<Guid> NotificationIds { get; set; } = [];
    }
}