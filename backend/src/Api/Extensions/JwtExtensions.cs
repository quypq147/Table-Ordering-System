using System.Text;
using Api.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Api.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddApiJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var jwt = config.GetSection("Jwt").Get<JwtOptions>()!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = false;
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
            options.AddPolicy("RequireStaffOrAdmin", p => p.RequireRole("Staff", "Admin"));
        });

        return services;
    }
}

