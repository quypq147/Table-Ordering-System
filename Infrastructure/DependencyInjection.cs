// Infrastructure/DependencyInjection.cs
using Domain.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config, bool usePostgres = false)
    {
        var cs = config.GetConnectionString("DefaultConnection")!;
        if (usePostgres)
            services.AddDbContext<TableOrderingDbContext>(o => o.UseNpgsql(cs));
        else
            services.AddDbContext<TableOrderingDbContext>(o => o.UseSqlServer(cs));

        // Repositories + UoW (EF)
        services.AddScoped<IOrderRepository, OrderRepositoryEf>();
        services.AddScoped<IMenuItemRepository, MenuItemRepositoryEf>();
        services.AddScoped<ITableRepository, TableRepositoryEf>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Seeder (tùy chọn gọi ở bootstrap)
        services.AddHostedService<SeedHostedService>();

        return services;
    }
}

// Hosted service để seed lúc khởi động
public sealed class SeedHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    public SeedHostedService(IServiceProvider sp) => _sp = sp;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TableOrderingDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        await DbSeeder.SeedAsync(db);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}



