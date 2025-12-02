using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider sp, IConfiguration cfg)
    {
        using var scope = sp.CreateScope();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var db = scope.ServiceProvider.GetRequiredService<TableOrderingDbContext>();

        await db.Database.MigrateAsync();

        foreach (var r in new[] { "Admin", "Staff" })
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new AppRole { Name = r });

        var adminEmail = cfg["Seed:AdminEmail"] ?? "admin@to.local";
        var adminUser = await userMgr.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Admin",
                IsActive = true
            };
            var pwd = cfg["Seed:AdminPassword"] ?? "Admin@123";
            var res = await userMgr.CreateAsync(adminUser, pwd);
            if (res.Succeeded)
                await userMgr.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
