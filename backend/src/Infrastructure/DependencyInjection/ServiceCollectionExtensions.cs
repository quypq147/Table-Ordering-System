using Application.Abstractions;
using Domain.Repositories;
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
        // DbContext
        services.AddDbContext<TableOrderingDbContext>(opt =>
        {
            var cs = config.GetConnectionString("DefaultConnection");
            opt.UseSqlServer(cs);
        });

        // ❌ BỎ: AddIdentityCore / AddJwtBearer / AddAuthorization (đưa sang API)

        // Repositories + UoW
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // IApplicationDbContext -> DbContext
        services.AddScoped<IApplicationDbContext, TableOrderingDbContext>();
        return services;
    }
}




