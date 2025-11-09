using Application.Abstractions;
using Infrastructure.SignalR;

namespace Api.DependencyInjection;

public static class KdsServiceCollectionExtensions
{
 public static IServiceCollection AddKdsServices(this IServiceCollection services)
 {
 services.AddSingleton<IKitchenTicketNotifier, KitchenTicketNotifier>();
 return services;
 }
}
