using Application.Abstractions;
using Domain.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Files;
using Infrastructure.DomainEvents;

namespace Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Domain events dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IApplicationDbContext, TableOrderingDbContext>();

        // DbContext
        services.AddDbContext<TableOrderingDbContext>(opt =>
        {
            var cs = config.GetConnectionString("DefaultConnection");
            opt.UseSqlServer(cs);
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




