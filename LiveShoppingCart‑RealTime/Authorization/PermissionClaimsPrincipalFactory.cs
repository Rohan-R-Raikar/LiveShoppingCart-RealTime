using LiveShoppingCart_RealTime.Data;
using LiveShoppingCart_RealTime.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace LiveShoppingCart_RealTime.Authorization
{
    public class PermissionClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public PermissionClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            ApplicationDbContext dbContext)
            : base(userManager, roleManager, optionsAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity;

            // Avoid duplicate claims
            var existingPermClaims = identity.FindAll("Permission").Select(c => c.Value).ToHashSet();

            // Fetch role names for user
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Count == 0) return principal;

            // Query all permissions for those roles
            var rolePermissions = await _dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => roles.Contains(rp.Role.Name))
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync();

            foreach (var perm in rolePermissions)
            {
                if (!existingPermClaims.Contains(perm))
                {
                    identity.AddClaim(new Claim("Permission", perm));
                }
            }

            return principal;
        }
    }
}
