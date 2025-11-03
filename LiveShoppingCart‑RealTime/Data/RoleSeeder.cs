using LiveShoppingCart_RealTime.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace LiveShoppingCart_RealTime.Data
{
    public class RoleSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
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
                    var identityRole = new IdentityRole(role);
                    await _roleManager.CreateAsync(identityRole);
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
    }
}
