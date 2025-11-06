using System.ComponentModel.DataAnnotations;

namespace LiveShoppingCart_RealTime.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public string Message { get; set; }

        public int? ParentMessageId { get; set; }
        public ChatMessage? ParentMessage { get; set; }

        public bool IsFlagged { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
