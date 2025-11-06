using System.ComponentModel.DataAnnotations;

namespace LiveShoppingCart_RealTime.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public Cart Cart { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; }

        public decimal PriceAtPurchase { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
