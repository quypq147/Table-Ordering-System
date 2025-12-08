using Microsoft.AspNetCore.SignalR.Client;

namespace WaiterApp.Services;

public sealed class OrdersRealtimeService
{
    private readonly AuthService _authService;
    private readonly ApiClient _apiClient;
    private readonly SemaphoreSlim _startStopLock = new(1, 1);
    private HubConnection? _connection;

    public event Action<Guid, string>? OrderStatusChanged;
    public event Action<string>? ConfigurationError;

    public OrdersRealtimeService(AuthService authService, ApiClient apiClient)
    {
        _authService = authService;
        _apiClient = apiClient;
    }

    public bool IsConnected => _connection is { State: HubConnectionState.Connected };

    public async Task StartAsync()
    {
        await _startStopLock.WaitAsync();
        try
        {
            if (_connection is { State: HubConnectionState.Connected or HubConnectionState.Connecting }) return;
            if (_apiClient.Http.BaseAddress is null)
            {
                ConfigurationError?.Invoke("Api base URL ch?a ???c c?u hình. Vui lòng ki?m tra c?u hình và th? l?i.");
                return;
            }

            var hubUrl = new Uri(_apiClient.Http.BaseAddress, "hubs/customer").ToString();

            // Build/start/join on a local instance first
            var conn = new HubConnectionBuilder()
                .WithUrl(hubUrl, o => o.AccessTokenProvider = () => Task.FromResult(_authService.Token))
                .WithAutomaticReconnect()
                .Build();

            conn.On<Guid, string>("orderStatusChanged", (orderId, status) => OrderStatusChanged?.Invoke(orderId, status));

            // Ensure we clean up the local connection if startup fails
            try
            {
                await conn.StartAsync();
                await conn.InvokeAsync("JoinStaffGroup");
            }
            catch
            {
                try
                {
                    await conn.DisposeAsync();
                }
                catch
                {
                    // ignore disposal exceptions during cleanup
                }

                throw;
            }

            // Atomically publish the fully-initialized connection
            var previous = Interlocked.Exchange(ref _connection, conn);
            if (previous != null)
            {
                // Clean up any previous connection if still present
                try
                {
                    await previous.StopAsync();
                }
                catch
                {
                    // ignore stop errors
                }

                try
                {
                    await previous.DisposeAsync();
                }
                catch
                {
                    // ignore dispose errors
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
        // Prevent concurrent Start/Stop
        await _startStopLock.WaitAsync();
        try
        {
            // Take ownership and null out the field
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
