using System.ComponentModel.DataAnnotations;

namespace LiveShoppingCart_RealTime.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string UserName { get; set; }
        
        [Required]
        public int ProductId { get; set; }
       
        [Required]
        public int Quantity { get; set; }

        public Product Product { get; set; }
    }
}
