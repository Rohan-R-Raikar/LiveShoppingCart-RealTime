using LiveShoppingCart_RealTime.Data;
using LiveShoppingCart_RealTime.Models;

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
            "CanChat"
        };

        foreach (var name in permissions)
        {
            if (!_context.Permissions.Any(p => p.Name == name))
            {
                _context.Permissions.Add(new Permission { Name = name });
            }
        }

        await _context.SaveChangesAsync();
    }
}
