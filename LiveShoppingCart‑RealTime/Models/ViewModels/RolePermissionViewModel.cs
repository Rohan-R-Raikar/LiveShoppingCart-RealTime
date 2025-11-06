namespace LiveShoppingCart_RealTime.Models.ViewModels
{
    public class RolePermissionViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }

        public List<Permission> AllPermissions { get; set; } = new();
        public List<int> SelectedPermissionIds { get; set; } = new();
    }
}
