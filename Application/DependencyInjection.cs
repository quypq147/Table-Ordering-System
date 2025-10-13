using Application.Common.CQRS;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();     // Mediator “nhẹ”
        services.AddCqrsHandlers();                // Quét & đăng ký toàn bộ Handler
        return services;
    }
}
