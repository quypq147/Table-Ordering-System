using Api.DependencyInjection;
using Api.Extensions;
using Api.Hubs;
using Application;
using Infrastructure.DependencyInjection;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders; // added for static files
using Application.Abstractions; // added
using Infrastructure.SignalR; // added
using Microsoft.AspNetCore.ResponseCompression; // compression
using Microsoft.AspNetCore.RateLimiting; // rate limiting
using System.Threading.RateLimiting; // rate limiting

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

// Add rate limiter with partitioned fixed window policy for QR scan endpoint
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("QrScanPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }
        )
    );

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Compression for JSON/SignalR (Server-Sent Events/WebSockets)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("adminweb", p =>
        p.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:5039"])
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


// Đăng ký notifier cho KDS  <<< PATCH MỚI
builder.Services.AddScoped<IKitchenTicketNotifier, KitchenTicketNotifier>();

builder.Services.AddApiIdentity();
builder.Services.AddApiJwtAuth(builder.Configuration);

builder.Services.AddKdsServices();
// customer notifier
builder.Services.AddSingleton<ICustomerNotifier, CustomerNotifier>();

// Health checks (DB)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TableOrderingDbContext>(customTestQuery: (db, ct) => db.Database.CanConnectAsync(ct));

var app = builder.Build();
app.UseCors("adminweb");

app.UseGlobalExceptionHandler();

app.UseResponseCompression();

// Enable rate limiting middleware
app.UseRateLimiter();

var webRoot = app.Environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
var uploadsRoot = Path.Combine(webRoot, "uploads");
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Cache static uploads aggressively
        var headers = ctx.Context.Response.Headers;
        headers["Cache-Control"] = "public,max-age=31536000,immutable";
    }
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
app.MapHub<CustomerHub>("/hubs/customer");

// Health endpoints
app.MapHealthChecks("/health");
app.MapGet("/health/db", async (TableOrderingDbContext db) =>
    await db.Database.CanConnectAsync() ? Results.Ok("OK") : Results.Problem("Không thể kết nối tới Database"));

await IdentitySeeder.SeedAsync(app.Services, app.Configuration);

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TableOrderingDbContext>();
    try
    {
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogError(ex, "Database migration failed at startup");
        // Optionally rethrow or continue based on policy
    }
}

app.Run();

