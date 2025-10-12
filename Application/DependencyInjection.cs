using Microsoft.Extensions.DependencyInjection;
using TableOrdering.Application.Orders;

namespace TableOrdering.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Đăng ký các UseCase, validator, mediator... ở đây
        services.AddScoped<OrderUseCases>();
        return services;
    }
}
