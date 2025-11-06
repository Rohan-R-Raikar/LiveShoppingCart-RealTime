namespace LiveShoppingCart_RealTime.Models
{
    public class ChatModerationLog
    {
        public int Id { get; set; }

        public int MessageId { get; set; }
        public ChatMessage Message { get; set; }

        public string DetectedBy { get; set; }
        public string Reason { get; set; }
        public string ActionTaken { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
