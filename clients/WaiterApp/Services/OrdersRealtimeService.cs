using Microsoft.AspNetCore.SignalR.Client;

namespace WaiterApp.Services;

public sealed class OrdersRealtimeService
{
    private const string MissingBaseUrlMessage = "API base URL is missing. Update it in Settings.";

    private readonly AuthService _authService;
    private readonly ApiClient _apiClient;
    private readonly SemaphoreSlim _startStopLock = new(1, 1);
    private HubConnection? _connection;

    public event Action<Guid, string>? OrderStatusChanged;
    public event Action<string>? ConfigurationError;

    // Chat message từ khách -> nhân viên
    public event Action<ChatMessagePayload>? ChatMessageReceived;

    // Payment request từ khách (Thanh toán tiền mặt)
    public event Action<PaymentRequestPayload>? PaymentRequestReceived;

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
                ConfigurationError?.Invoke(MissingBaseUrlMessage);
                return;
            }

            var hubUrl = new Uri(_apiClient.Http.BaseAddress, "hubs/customer").ToString();

            // Build/start/join on a local instance first
            var conn = new HubConnectionBuilder()
                .WithUrl(hubUrl, o => o.AccessTokenProvider = () => Task.FromResult(_authService.Token))
                .WithAutomaticReconnect()
                .Build();

            conn.On<Guid, string>("orderStatusChanged", (orderId, status) => OrderStatusChanged?.Invoke(orderId, status));

            // Lắng nghe chatMessage: payload ẩn danh { tableId, sender, message, sentAtUtc }
            conn.On<ChatMessagePayload>("chatMessage", payload =>
            {
                ChatMessageReceived?.Invoke(payload);
            });

            // Lắng nghe yêu cầu thanh toán tiền mặt từ khách
            conn.On<PaymentRequestPayload>("ReceivePaymentRequest", payload =>
             {
                 PaymentRequestReceived?.Invoke(payload);
             });

            // Ensure we clean up the local connection if startup fails
            try
            {
                await conn.StartAsync();
                // Nhóm staff để nhận mọi chat của khách và yêu cầu thanh toán
                await conn.InvokeAsync("JoinStaffGroup", _authService.Token);
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

    // Add this method to OrdersRealtimeService if it is missing
    public void On<T>(string eventName, Action<T> handler)
    {
        // Implementation depends on your realtime infrastructure (e.g., SignalR, WebSocket, etc.)
        // For example, if using SignalR:
        // _hubConnection.On<T>(eventName, handler);

        // Placeholder: throw if not implemented
        throw new NotImplementedException("On<T> method must be implemented to subscribe to realtime events.");
    }
}

// DTO đơn giản để nhận payload chat từ Hub
public sealed class ChatMessagePayload
{
    public string TableId { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}

// DTO cho yêu cầu thanh toán tiền mặt từ khách
public sealed class PaymentRequestPayload
{
    public Guid OrderId { get; set; }
    public string TableCode { get; set; } = string.Empty;
}
