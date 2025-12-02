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

    public bool IsConnected =>
        _connection is { State: HubConnectionState.Connected };

    /// <summary>
    /// K?t n?i t?i KDS Hub (/hubs/kds) ?? nh?n ticket realtime
    /// </summary>
    public async Task StartAsync()
    {
        // N?u ?ã k?t n?i r?i thì thôi, tránh t?o nhi?u connection
        if (_connection is { State: HubConnectionState.Connected or HubConnectionState.Connecting })
            return;

        // Xây URL hub d?a trên BaseAddress c?a ApiClient
        if (_apiClient.Http.BaseAddress is null)
            throw new InvalidOperationException("ApiClient.BaseAddress ch?a ???c c?u hình.");

        var hubUrl = new Uri(_apiClient.Http.BaseAddress, "hubs/kds").ToString();

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options => { options.AccessTokenProvider = () => Task.FromResult(_authService.Token); })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<KitchenTicketDto[]>("ticketsCreated", tickets => TicketsCreated?.Invoke(tickets));
        _connection.On<KitchenTicketDto>("ticketChanged", ticket => TicketChanged?.Invoke(ticket));

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
