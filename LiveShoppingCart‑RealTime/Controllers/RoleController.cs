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
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RoleController(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        // GET: List all users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
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
                var result = await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
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

        // GET: Assign permissions to a role
        public async Task<IActionResult> AssignPermissions(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
                return NotFound();

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound();

            var allPermissions = await _context.Permissions.ToListAsync();
            var selectedIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var model = new RolePermissionViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                AllPermissions = allPermissions,
                SelectedPermissionIds = selectedIds
            };

            return View(model);
        }

        // POST: Assign permissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPermissions(RolePermissionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Rehydrate AllPermissions for redisplay
                model.AllPermissions = await _context.Permissions.ToListAsync();
                return View(model);
            }

            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
                return NotFound();

            // Remove existing permissions
            var existing = await _context.RolePermissions.Where(rp => rp.RoleId == model.RoleId).ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            // Add newly selected permissions safely
            if (model.SelectedPermissionIds != null && model.SelectedPermissionIds.Any())
            {
                foreach (var permissionId in model.SelectedPermissionIds)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = model.RoleId,
                        PermissionId = permissionId
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Permissions updated for role '{role.Name}'.";

            return RedirectToAction(nameof(Index));
        }


        // GET: Assign roles to a user
        public async Task<IActionResult> AssignRoles(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            var allRoles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                AllRoles = allRoles,
                SelectedRoleIds = allRoles
                                    .Where(r => userRoles.Contains(r.Name))
                                    .Select(r => r.Id)
                                    .ToList()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoles(UserRolesViewModel model)
        {
            model.AllRoles = _roleManager.Roles.ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove all existing roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                return View(model);
            }

            // Assign newly selected roles
            if (model.SelectedRoleIds != null && model.SelectedRoleIds.Any())
            {
                var rolesToAdd = _roleManager.Roles
                                             .Where(r => model.SelectedRoleIds.Contains(r.Id))
                                             .Select(r => r.Name)
                                             .ToList();

                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    ModelState.AddModelError("", string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    return View(model);
                }
            }

            TempData["Success"] = $"Roles updated for user '{user.UserName}'.";
            return RedirectToAction(nameof(Index));
        }


    }
}
