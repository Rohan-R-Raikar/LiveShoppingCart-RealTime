using Microsoft.AspNetCore.Identity;

namespace LiveShoppingCart_RealTime.Models
{
    public class ApplicationRole : IdentityRole
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
