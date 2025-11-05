using LiveShoppingCart_RealTime.Models;

namespace LiveShoppingCart_RealTime.Data
{
    public class PermissionSeeder
    {
        private readonly ApplicationDbContext _context;

        public PermissionSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Seed()
        {
            if (!_context.Permissions.Any())
            {
                var permissions = new[]
                {
                    new Permission { Name = "CanAddProduct" },
                    new Permission { Name = "CanEditProduct" },
                    new Permission { Name = "CanDeleteProduct" },
                    new Permission { Name = "CanChat" }
                };

                _context.Permissions.AddRange(permissions);
                _context.SaveChanges();
            }
        }
    }
}
