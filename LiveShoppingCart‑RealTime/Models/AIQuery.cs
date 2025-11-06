namespace LiveShoppingCart_RealTime.Models
{
    public class AIQuery
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public string QueryText { get; set; }
        public string? ResponseText { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
