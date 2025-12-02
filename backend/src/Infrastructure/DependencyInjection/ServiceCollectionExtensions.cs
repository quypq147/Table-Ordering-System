using Application.Abstractions;
using Domain.Repositories;
using Infrastructure.DomainEvents;
using Infrastructure.Files;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Domain events dispatcher (singleton so it can be injected into pooled DbContext)
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IApplicationDbContext, TableOrderingDbContext>();

        // DbContext (pooled + resilient SQL connection)
        services.AddDbContextPool<TableOrderingDbContext>(opt =>
        {
            var cs = config.GetConnectionString("DefaultConnection");
            opt.UseSqlServer(cs, sql =>
            {
                sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                sql.CommandTimeout(30);
            });
        });

        // Repositories + UoW
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // File storage (local by default)
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }
}




