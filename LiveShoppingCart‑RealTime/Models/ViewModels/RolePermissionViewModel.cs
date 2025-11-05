using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LiveShoppingCart_RealTime.Models.ViewModels
{
    public class RolePermissionViewModel
    {
        [Required]
        public string RoleId { get; set; }

        [Display(Name = "Role Name")]
        public string RoleName { get; set; }

        [NotMapped]
        public List<Permission> AllPermissions { get; set; } = new List<Permission>();

        [Display(Name = "Selected Permissions")]
        public List<int> SelectedPermissionIds { get; set; } = new List<int>();
    }
}
