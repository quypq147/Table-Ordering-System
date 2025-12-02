using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Api.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddApiIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<AppUser>(o =>
        {
            o.User.RequireUniqueEmail = true;
            o.Password.RequiredLength = 6;
            o.Password.RequireDigit = false;
            o.Password.RequireUppercase = false;
            o.Password.RequireNonAlphanumeric = false;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<TableOrderingDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        return services;
    }
}

