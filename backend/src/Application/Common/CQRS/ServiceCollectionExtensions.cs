// Application/Common/CQRS/ServiceCollectionExtensions.cs
using Application;
using Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCqrsHandlers(this IServiceCollection services)
    {
        var asm = typeof(DependencyInjection).Assembly;

        foreach (var type in asm.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface) continue;

            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType) continue;
                var gen = iface.GetGenericTypeDefinition();

                // 1) Command + Query handlers
                if (gen == typeof(ICommandHandler<,>) || gen == typeof(IQueryHandler<,>))
                {
                    services.AddScoped(iface, type);
                }

                // 2) Domain Event handlers  <<< PATCH MỚI
                if (gen == typeof(IDomainEventHandler<>))
                {
                    services.AddScoped(iface, type);
                }
            }
        }

        return services;
    }
}


