using Infrastructure.Identity;
using Infrastructure.DependencyInjection;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders; // added for static files
using System.IO; // added for Directory
using Api.Extensions;
using Api.Hubs;
using Api.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(opt =>
{
 opt.AddPolicy("adminweb", p =>
 p.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new []{"http://localhost:5039"})
 .AllowAnyHeader()
 .AllowAnyMethod()
 .AllowCredentials());
});
builder.Services.AddSwaggerGen(c =>
{
 c.CustomSchemaIds(t => t.FullName!.Replace('+', '.'));
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddApiIdentity();
builder.Services.AddApiJwtAuth(builder.Configuration);

builder.Services.AddKdsServices();

var app = builder.Build();
app.UseCors("adminweb");

app.UseGlobalExceptionHandler();

var webRoot = app.Environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
Directory.CreateDirectory(Path.Combine(webRoot, "uploads"));
app.UseStaticFiles(new StaticFileOptions
{
 FileProvider = new PhysicalFileProvider(Path.Combine(webRoot, "uploads")),
 RequestPath = "/uploads"
});

if (app.Environment.IsDevelopment())
{
 app.UseSwagger();
 app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<KdsHub>("/hubs/kds");
app.MapGet("/health/db", async (TableOrderingDbContext db) =>
 await db.Database.CanConnectAsync() ? Results.Ok("OK") : Results.Problem("Không thể kết nối tới Database"));

await IdentitySeeder.SeedAsync(app.Services, app.Configuration);

app.Run();

