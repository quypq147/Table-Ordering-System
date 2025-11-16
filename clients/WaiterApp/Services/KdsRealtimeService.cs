using Microsoft.AspNetCore.SignalR.Client;
using WaiterApp.Models;

namespace WaiterApp.Services;

public class KdsRealtimeService
{
    private readonly AuthService _authService;
    private readonly ApiClient _apiClient;
    private HubConnection? _connection;

    public event Action<KitchenTicketDto[]>? TicketsCreated;
    public event Action<KitchenTicketDto>? TicketChanged;

    public KdsRealtimeService(AuthService authService, ApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
    }

    public async Task StartAsync()
    {
        if (_connection is not null &&
            (_connection.State == HubConnectionState.Connected ||
             _connection.State == HubConnectionState.Connecting))
            return;

        var hubUrl = $"{_apiClient.Http.BaseAddress}hubs/kds";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () =>
                    Task.FromResult(_authService.Token);
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<IEnumerable<KitchenTicketDto>>("ticketsCreated", tickets =>
        {
            TicketsCreated?.Invoke(tickets.ToArray());
        });

        _connection.On<KitchenTicketDto>("ticketChanged", ticket =>
        {
            TicketChanged?.Invoke(ticket);
        });

        await _connection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
