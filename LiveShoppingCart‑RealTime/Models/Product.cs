using System.ComponentModel.DataAnnotations;

namespace LiveShoppingCart_RealTime.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(1, 500000)]
        public decimal Price { get; set; }

        [Required]
        public int Stock {  get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
    }
}
