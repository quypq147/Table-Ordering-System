using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.ApplicationModel; // for MainThread

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

            var conn = new HubConnectionBuilder()
                .WithUrl(hubUrl, o =>
                {
                    // Always use latest token when (re)connecting
                    o.AccessTokenProvider = () => Task.FromResult(_authService.Token);
                })
                .WithAutomaticReconnect()
                .Build();

            // Accept orderId as string and parse to Guid to avoid type issues
            conn.On<string, string>("orderStatusChanged", (orderIdStr, status) =>
            {
                if (!Guid.TryParse(orderIdStr, out var orderId)) return;

                System.Diagnostics.Debug.WriteLine($"[SignalR] Order {orderId} updated to {status}");
                MainThread.BeginInvokeOnMainThread(() => OrderStatusChanged?.Invoke(orderId, status));
            });

            // Chat message from customer -> staff
            conn.On<ChatMessagePayload>("chatMessage", payload =>
            {
                ChatMessageReceived?.Invoke(payload);
            });

            // Cash payment request from customer
            conn.On<PaymentRequestPayload>("ReceivePaymentRequest", payload =>
            {
                PaymentRequestReceived?.Invoke(payload);
            });

            conn.Reconnected += async _ =>
            {
                System.Diagnostics.Debug.WriteLine("SignalR Reconnected. Rejoining staff group...");
                try
                {
                    await conn.InvokeAsync("JoinStaffGroup");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error rejoining staff group: {ex.Message}");
                }
            };

            try
            {
                await conn.StartAsync();
                await conn.InvokeAsync("JoinStaffGroup");
            }
            catch
            {
                try { await conn.DisposeAsync(); } catch { }
                throw;
            }

            var previous = Interlocked.Exchange(ref _connection, conn);
            if (previous != null)
            {
                try { await previous.StopAsync(); } catch { }
                try { await previous.DisposeAsync(); } catch { }
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

    // Helper to subscribe to hub events in a safe way
    public void On<T>(string eventName, Action<T> handler)
    {
        var conn = _connection;
        if (conn == null)
        {
            // No active connection yet; nothing to wire
            return;
        }

        // Register handler on the underlying SignalR connection
        conn.On(eventName, handler);
    }
}

// DTO đơn giản để nhận payload chat từ Hub
public sealed class ChatMessagePayload
{
    public string TableId { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string ReceiverId { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}

// DTO cho yêu cầu thanh toán tiền mặt từ khách
public sealed class PaymentRequestPayload
{
    public Guid OrderId { get; set; }
    public string TableCode { get; set; } = string.Empty;
}
