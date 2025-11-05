namespace LiveShoppingCart_RealTime.Models.ViewModels
{
    public class RolePermissionViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<Permission> Permissions { get; set; } = new List<Permission>();
        public List<int> SelectedPermissionIds { get; set; } = new List<int>();
    }
}
