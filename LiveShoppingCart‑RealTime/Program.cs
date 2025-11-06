using LiveShoppingCart_RealTime.Data;
using LiveShoppingCart_RealTime.Hubs;
using LiveShoppingCart_RealTime.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register RoleSeeder service
builder.Services.AddScoped<RoleSeeder>();

builder.Services.AddScoped<PermissionSeeder>();

// For database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Identity (login system) ---
// Using default IdentityUser
//builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
//    options.SignIn.RequireConfirmedAccount = false)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// --- MVC + SignalR ---
builder.Services.AddSignalR();

var app = builder.Build();

// --- Seed Roles & Admin via Service ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleSeeder = new RoleSeeder(
    services.GetRequiredService<RoleManager<ApplicationRole>>(),
    services.GetRequiredService<UserManager<ApplicationUser>>(),
    services.GetRequiredService<ApplicationDbContext>()
);

        await roleSeeder.SeedRolesAsync();
        await roleSeeder.SeedAdminAsync("admin@shop.com", "Admin@123");
        await roleSeeder.SeedDefaultPermissionsAsync();

        var permissionSeeder = new PermissionSeeder(
            services.GetRequiredService<ApplicationDbContext>()
        );
        await permissionSeeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles and admin user.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chathub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
