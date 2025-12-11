using Microsoft.AspNetCore.SignalR.Client;
using WaiterApp.Models;

namespace WaiterApp.Services;

public class KdsRealtimeService
{
    private const string MissingBaseUrlMessage = "API base URL is missing. Update it in Settings.";

    private readonly AuthService _authService;
    private readonly ApiClient _apiClient;
    private readonly SemaphoreSlim _startStopLock = new(1, 1);
    private HubConnection? _connection;

    public event Action<KitchenTicketDto[]>? TicketsCreated;
    public event Action<KitchenTicketDto>? TicketChanged;
    public event Action<string>? ConfigurationError;

    public KdsRealtimeService(AuthService authService, ApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
    }

    public bool IsConnected =>
        _connection is { State: HubConnectionState.Connected };

    /// <summary>
    /// Connect to the KDS Hub (/hubs/kds) to receive realtime kitchen tickets.
    /// </summary>
    public async Task StartAsync()
    {
        await _startStopLock.WaitAsync();
        try
        {
            if (_connection is { State: HubConnectionState.Connected or HubConnectionState.Connecting })
                return;

            if (_apiClient.Http.BaseAddress is null)
            {
                ConfigurationError?.Invoke(MissingBaseUrlMessage);
                return;
            }

            var hubUrl = new Uri(_apiClient.Http.BaseAddress, "hubs/kds").ToString();

            var conn = new HubConnectionBuilder()
                .WithUrl(hubUrl, options => { options.AccessTokenProvider = () => Task.FromResult(_authService.Token); })
                .WithAutomaticReconnect()
                .Build();

            conn.On<KitchenTicketDto[]>("ticketsCreated", tickets => TicketsCreated?.Invoke(tickets));
            conn.On<KitchenTicketDto>("ticketChanged", ticket => TicketChanged?.Invoke(ticket));

            try
            {
                await conn.StartAsync();
            }
            catch
            {
                try
                {
                    await conn.DisposeAsync();
                }
                catch
                {
                    // ignore disposal failures
                }

                throw;
            }

            var previous = Interlocked.Exchange(ref _connection, conn);
            if (previous != null)
            {
                try
                {
                    await previous.StopAsync();
                }
                catch
                {
                    // ignore stop failures
                }

                try
                {
                    await previous.DisposeAsync();
                }
                catch
                {
                    // ignore dispose failures
                }
            }
        }
        finally
        {
            _startStopLock.Release();
        }
    }

    public async Task StopAsync()
    {
        await _startStopLock.WaitAsync();
        try
        {
            var conn = Interlocked.Exchange(ref _connection, null);
            if (conn != null)
            {
                await conn.StopAsync();
                await conn.DisposeAsync();
            }
        }
        finally
        {
            _startStopLock.Release();
        }
    }
}
