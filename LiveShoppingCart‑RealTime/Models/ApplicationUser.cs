using Microsoft.AspNetCore.Identity;

namespace LiveShoppingCart_RealTime.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
