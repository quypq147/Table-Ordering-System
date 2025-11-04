using MediatR;
using Application.Common.CQRS;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<IOrderCodeGenerator, Services.ShortFriendlyOrderCodeGenerator>();
        services.AddScoped<Common.CQRS.ISender, Sender>();     // Mediator “nhẹ”
        services.AddCqrsHandlers();                // Quét & đăng ký toàn bộ Handler
        return services;
    }
}
