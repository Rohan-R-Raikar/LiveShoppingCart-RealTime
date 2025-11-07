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
        private readonly ILogger<RoleController> _logger;

        public RoleController(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<RoleController> logger)
        {
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: List all users
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                _logger.LogInformation("Fetched {UserCount} users successfully.", users.Count);
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users.");
                ModelState.AddModelError("", "Something went wrong while fetching Users.");
                return View();
            }
        }

        // List all roles
        public IActionResult Index()
        {
            try
            {
                var roles = _roleManager.Roles.ToList();
                _logger.LogInformation("Fetched {RoleCount} roles successfully.", roles.Count);
                return View(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching roles.");
                ModelState.AddModelError("", "Something went wrong while fetching Roles.");
                return View();
            }
        }

        // Create Role Page
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    _logger.LogError(roleName, "RoleName is Empty");
                    ModelState.AddModelError("", "Role name cannot be empty");

                    TempData["SweetAlertMessage"] = "Something went wrong!";
                    TempData["SweetAlertType"] = "error";

                    return View();
                }

                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var result = await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("New Role Added Successfully");

                        TempData["SweetAlertMessage"] = "Role created successfully!";
                        TempData["SweetAlertType"] = "success";

                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError(roleName, "Not Found");
                        ModelState.AddModelError("", string.Join(", ", result.Errors.Select(e => e.Description)));

                        TempData["SweetAlertMessage"] = "Role Not Found";
                        TempData["SweetAlertType"] = "error";
                    }
                }
                else
                {
                    _logger.LogError(roleName, "Role already exists");
                    ModelState.AddModelError("", "Role already exists");

                    TempData["SweetAlertMessage"] = "Role already exists!";
                    TempData["SweetAlertType"] = "error";
                }

                return View();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception Occured while Creating Role");
                ModelState.AddModelError("", "Exception Occured while Creating Role");

                TempData["SweetAlertMessage"] = "Something went wrong!";
                TempData["SweetAlertType"] = "error";

                return View();
            }
        }

        // GET: Assign permissions to a role
        public async Task<IActionResult> AssignPermissions(string roleId)
        {
            try
            {
                if (string.IsNullOrEmpty(roleId))
                {
                    _logger.LogError(roleId, "RoleID not Found");
                    return NotFound();
                }

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogError("Role is Null here");
                    return NotFound();
                }

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

                _logger.LogInformation(roleId,"Passing the Permision list of a Role");
                return View(model);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Error Ocured while fetching Permissions from Role");
                return View();
            }
        }

        // POST: Assign permissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPermissions(RolePermissionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Model State is InValid");
                    model.AllPermissions = await _context.Permissions.ToListAsync();
                    return View(model);
                }

                var role = await _roleManager.FindByIdAsync(model.RoleId);
                if (role == null)
                {
                    _logger.LogError("Role is Null here");
                    return NotFound();
                }

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

                _logger.LogInformation("Permissions updated for role successfully!");
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Permissions updated for role '{role.Name}'.";
                TempData["SweetAlertMessage"] = "Permissions updated for role successfully!";
                TempData["SweetAlertType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View(model);
            }
        }


        // GET: Assign roles to a user
        public async Task<IActionResult> AssignRoles(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError(userId, "UserId ss Null or Empty here");
                    return NotFound();
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogError("User Not found");
                    return NotFound();
                }

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
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error Occured while Assigning Role");
                return RedirectToAction(nameof(Index), ex);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoles(UserRolesViewModel model)
        {
            try
            {

                model.AllRoles = _roleManager.Roles.ToList();

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Model State is InValid");
                    return View(model);
                }

                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null)
                {
                    _logger.LogError("user is null here");
                    return NotFound();
                }

                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove all existing roles
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    _logger.LogError("Error Occured while removing Old Roles");
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
                TempData["SweetAlertMessage"] = "Role created successfully!";
                TempData["SweetAlertType"] = "success";

                _logger.LogInformation("Role Assigned successfully!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Assigning Role");
                return View(model);
            }
        }


    }
}
