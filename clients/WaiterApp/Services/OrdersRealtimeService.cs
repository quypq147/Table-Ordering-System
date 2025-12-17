using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.ApplicationModel;
using WaiterApp.Models;

namespace WaiterApp.Services;

public sealed class OrdersRealtimeService
{
    private const string MissingBaseUrlMessage = "Chưa cấu hình API URL.";
    private readonly AuthService _authService;
    private readonly ApiClient _apiClient;
    private readonly SemaphoreSlim _startStopLock = new(1, 1);
    private HubConnection? _connection;

    public event Action<Guid, string>? OrderStatusChanged;
    public event Action<string>? ConfigurationError;
    public event Action<ChatMessagePayload>? ChatMessageReceived;
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

            // FIX: Xử lý URL an toàn hơn để tránh lỗi nếu BaseAddress có/không có dấu gạch chéo
            var baseUrl = _apiClient.Http.BaseAddress.ToString().TrimEnd('/');
            var hubUrl = $"{baseUrl}/hubs/customer";

            var conn = new HubConnectionBuilder()
                .WithUrl(hubUrl, o =>
                {
                    o.AccessTokenProvider = () => Task.FromResult(_authService.Token);
                })
                .WithAutomaticReconnect()
                .Build();

            // Lắng nghe sự kiện từ API (ApiCustomerNotifier gửi 2 tham số)
            conn.On<Guid, string>("orderStatusChanged", (orderId, status) =>
            {
                System.Diagnostics.Debug.WriteLine($"[SignalR] Order {orderId} -> {status}");
                MainThread.BeginInvokeOnMainThread(() => OrderStatusChanged?.Invoke(orderId, status));
            });

            // FIX: Fallback lắng nghe sự kiện dạng Object (nếu Infrastructure Notifier bị dùng nhầm)
            conn.On<object>("orderStatusChanged", (payload) =>
            {
                // Logic parse object payload thủ công nếu cần thiết để backup
                System.Diagnostics.Debug.WriteLine("[SignalR] Nhận payload object (Legacy/Wrong Notifier)");
            });

            conn.On<ChatMessagePayload>("chatMessage", p => ChatMessageReceived?.Invoke(p));
            conn.On<PaymentRequestPayload>("ReceivePaymentRequest", p => PaymentRequestReceived?.Invoke(p));

            conn.Reconnected += async _ => await JoinStaffGroupSafe(conn);

            await conn.StartAsync();
            await JoinStaffGroupSafe(conn);

            var previous = Interlocked.Exchange(ref _connection, conn);
            if (previous != null) await previous.DisposeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SignalR] Connection Error: {ex.Message}");
        }
        finally
        {
            _startStopLock.Release();
        }
    }

    private async Task JoinStaffGroupSafe(HubConnection conn)
    {
        try
        {
            await conn.InvokeAsync("JoinStaffGroup");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SignalR] JoinStaffGroup Failed: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        await _startStopLock.WaitAsync();
        try
        {
            var conn = Interlocked.Exchange(ref _connection, null);
            if (conn != null) await conn.DisposeAsync();
        }
        finally { _startStopLock.Release(); }
    }
}