using LiveShoppingCart_RealTime.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace LiveShoppingCart_RealTime.Data
{
    public class RoleSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RoleSeeder(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _context = context;
        }


        /// <summary>
        /// Seeds default roles: Admin and User
        /// </summary>
        public async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var appRole = new ApplicationRole { Name = role };
                    await _roleManager.CreateAsync(appRole);
                }
            }
        }

        /// <summary>
        /// Seeds a default Admin user
        /// </summary>
        public async Task SeedAdminAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Admin email cannot be empty.", nameof(email));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Admin password cannot be empty.", nameof(password));

            var existingAdmin = await _userManager.FindByEmailAsync(email);
            if (existingAdmin != null)
                return;

            var adminUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors);
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }

            await _userManager.AddToRoleAsync(adminUser, "Admin");
        }

        /// <summary>
        /// Assigns the "User" role to newly registered users
        /// (to be called after user registration)
        /// </summary>
        public async Task AssignUserRoleAsync(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
        }


        public async Task SeedDefaultPermissionsAsync()
        {
            // 1. Add permissions if they do not exist
            var defaultPermissions = new List<Permission>
            {
                new Permission { Name = "CanAddToCart"},
                new Permission { Name = "CanChatInCart" }
            };

            foreach (var perm in defaultPermissions)
            {
                if (!await _context.Permissions.AnyAsync(p => p.Name == perm.Name))
                {
                    _context.Permissions.Add(perm);
                }
            }
            await _context.SaveChangesAsync();

            // 2. Assign permissions to the User role
            var userRole = await _roleManager.FindByNameAsync("User");
            var permissions = await _context.Permissions
                                    .Where(p => defaultPermissions.Select(dp => dp.Name).Contains(p.Name))
                                    .ToListAsync();

            foreach (var perm in permissions)
            {
                if (!_context.RolePermissions.Any(rp => rp.RoleId == userRole.Id && rp.PermissionId == perm.Id))
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = userRole.Id,
                        PermissionId = perm.Id
                    });
                }
            }
            await _context.SaveChangesAsync();
        }


    }
}
