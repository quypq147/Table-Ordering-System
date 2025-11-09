using MediatR;
using Application.Common.CQRS;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Pipeline;
using FluentValidation;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<IOrderCodeGenerator, Services.ShortFriendlyOrderCodeGenerator>();
        services.AddScoped<Common.CQRS.ISender, Sender>(); // Mediator “nhẹ”
        services.AddCqrsHandlers(); // Quét & đăng ký toàn bộ Handler

        // Pipeline behaviors
        services.AddScoped<IRequestBehavior, LoggingBehavior>();
        services.AddScoped<IRequestBehavior, PerformanceBehavior>();
        services.AddScoped<IRequestBehavior, ValidationBehavior>();

        // Manually register validators (scan assembly types)
        foreach (var type in typeof(DependencyInjection).Assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface) continue;
            var validatorInterface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
            if (validatorInterface is not null)
            {
                services.AddScoped(validatorInterface, type);
            }
        }

        return services;
    }
}
