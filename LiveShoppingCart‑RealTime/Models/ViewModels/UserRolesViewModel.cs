using LiveShoppingCart_RealTime.Models;

namespace LiveShoppingCart_RealTime.Models.ViewModels
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public List<ApplicationRole> AllRoles { get; set; } = new();
        public List<string> SelectedRoleIds { get; set; } = new();
    }
}
