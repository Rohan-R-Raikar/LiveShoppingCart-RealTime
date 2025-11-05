using LiveShoppingCart_RealTime.Data;
using LiveShoppingCart_RealTime.Models;
using LiveShoppingCart_RealTime.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveShoppingCart_RealTime.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RoleController(RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        // List all roles
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        // Create Role Page
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("", "Role name cannot be empty");
                return View();
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));
                else
                    ModelState.AddModelError("", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            else
            {
                ModelState.AddModelError("", "Role already exists");
            }

            return View();
        }

        // Show form to assign permissions to a role
        public async Task<IActionResult> AssignPermissions(string roleId)
        {
            if (roleId == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound();

            var allPermissions = await _context.Permissions.ToListAsync();
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var model = new RolePermissionViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Permissions = allPermissions,
                SelectedPermissionIds = rolePermissions
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignPermissions(RolePermissionViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Remove existing permissions
            var existingPermissions = _context.RolePermissions.Where(rp => rp.RoleId == model.RoleId);
            _context.RolePermissions.RemoveRange(existingPermissions);

            // Add new permissions
            foreach (var permissionId in model.SelectedPermissionIds)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = model.RoleId,
                    PermissionId = permissionId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
