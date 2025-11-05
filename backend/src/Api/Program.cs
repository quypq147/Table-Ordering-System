using Infrastructure.Identity;
using Infrastructure.DependencyInjection;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("adminweb", p =>
        p.WithOrigins("http://localhost:5039") // port AdminWeb
         .AllowAnyHeader()
         .AllowAnyMethod());
});
builder.Services.AddSwaggerGen(c =>
{
    // Use FullName to avoid schemaId collisions for nested types (nested types use '+')
    c.CustomSchemaIds(t => t.FullName!.Replace('+', '.'));
});

// Infrastructure (DbContext + repositories only)
builder.Services.AddInfrastructure(builder.Configuration);

// Application layer
builder.Services.AddApplication();

// Identity (Guid keys)
builder.Services.AddIdentityCore<AppUser>(o =>
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

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
 {
     opt.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtAudience,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
         ClockSkew = TimeSpan.FromMinutes(1)
     };
     opt.MapInboundClaims = false;
 });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
    options.AddPolicy("RequireStaffOrAdmin", p => p.RequireRole("Staff", "Admin"));
});

var app = builder.Build();
app.UseCors("adminweb");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health/db", async (TableOrderingDbContext db) =>
    await db.Database.CanConnectAsync() ? Results.Ok("OK") : Results.Problem("Không thể kết nối tới Database"));

// Identity seeding (also performs migration)
await IdentitySeeder.SeedAsync(app.Services, app.Configuration);

app.Run();

