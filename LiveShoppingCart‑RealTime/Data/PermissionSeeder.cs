using LiveShoppingCart_RealTime.Data;
using LiveShoppingCart_RealTime.Models;
using Microsoft.EntityFrameworkCore;

public class PermissionSeeder
{
    private readonly ApplicationDbContext _context;

    public PermissionSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var permissions = new[]
        {
            "CanAddProduct",
            "CanEditProduct",
            "CanDeleteProduct",
            "CanChat",
            "CanManageRoles",
            "CanManagePermissions",
            "CanAddCategory",
            "CanEditCategory",
            "CanDeleteCategory"
        };

        foreach (var name in permissions)
        {
            if (!await _context.Permissions.AnyAsync(p => p.Name == name))
            {
                _context.Permissions.Add(new Permission { Name = name });
            }
        }

        await _context.SaveChangesAsync();
    }
}
