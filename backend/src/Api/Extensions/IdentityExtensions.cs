using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddApiIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(o =>
        {
            o.User.RequireUniqueEmail = true;
            o.Password.RequiredLength = 6;
            o.Password.RequireDigit = false;
            o.Password.RequireUppercase = false;
            o.Password.RequireNonAlphanumeric = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<TableOrderingDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        return services;
    }
}

