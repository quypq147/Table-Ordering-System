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
            {
                System.Diagnostics.Debug.WriteLine("[KdsRealtime] StartAsync: already connected or connecting.");
                return;
            }

            if (_apiClient.Http.BaseAddress is null)
            {
                System.Diagnostics.Debug.WriteLine("[KdsRealtime] StartAsync: missing BaseAddress.");
                ConfigurationError?.Invoke(MissingBaseUrlMessage);
                return;
            }

            var hubUrl = new Uri(_apiClient.Http.BaseAddress, "hubs/kds").ToString();
            System.Diagnostics.Debug.WriteLine($"[KdsRealtime] Building connection to {hubUrl}");

            var conn = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () =>
                    {
                        var token = _authService.Token;
                        System.Diagnostics.Debug.WriteLine($"[KdsRealtime] AccessTokenProvider called. HasToken={(string.IsNullOrWhiteSpace(token) ? "false" : "true")}");
                        return Task.FromResult(token);
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            conn.Reconnecting += error =>
            {
                System.Diagnostics.Debug.WriteLine($"[KdsRealtime] Reconnecting: {error?.Message}");
                return Task.CompletedTask;
            };

            conn.Reconnected += connectionId =>
            {
                System.Diagnostics.Debug.WriteLine($"[KdsRealtime] Reconnected. ConnectionId={connectionId}");
                return Task.CompletedTask;
            };

            conn.Closed += error =>
            {
                System.Diagnostics.Debug.WriteLine($"[KdsRealtime] Closed: {error?.Message}");
                return Task.CompletedTask;
            };

            conn.On<KitchenTicketDto[]>("ticketsCreated", tickets =>
            {
                System.Diagnostics.Debug.WriteLine($"[KdsRealtime] Event ticketsCreated: count={tickets?.Length ?? 0}");
                TicketsCreated?.Invoke(tickets);
            });

            conn.On<KitchenTicketDto>("ticketChanged", ticket =>
            {
                System.Diagnostics.Debug.WriteLine($"[KdsRealtime] Event ticketChanged: ticketId={ticket.Id}");
                TicketChanged?.Invoke(ticket);
            });

            try
            {
                System.Diagnostics.Debug.WriteLine("[KdsRealtime] Starting connection...");
                await conn.StartAsync();
                System.Diagnostics.Debug.WriteLine($"[KdsRealtime] Connection started. State={conn.State}, ConnectionId={conn.ConnectionId}");
            }
            catch
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[KdsRealtime] StartAsync failed, disposing connection.");
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
                System.Diagnostics.Debug.WriteLine("[KdsRealtime] Disposing previous connection instance.");
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
                System.Diagnostics.Debug.WriteLine("[KdsRealtime] Stopping and disposing connection.");
                await conn.StopAsync();
                await conn.DisposeAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[KdsRealtime] StopAsync called but there is no active connection.");
            }
        }
        finally
        {
            _startStopLock.Release();
        }
    }
}
