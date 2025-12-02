using Application.Abstractions;
using Infrastructure.SignalR;

namespace Api.DependencyInjection;

public static class KdsServiceCollectionExtensions
{
    public static IServiceCollection AddKdsServices(this IServiceCollection services)
    {
        // ??ng ký notifier dùng KdsHub trong Api
        services.AddSingleton<IKitchenTicketNotifier, Api.SignalR.ApiKitchenTicketNotifier>();
        return services;
    }
}
