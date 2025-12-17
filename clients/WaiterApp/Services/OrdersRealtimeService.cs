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
            if (_connection is { State: HubConnectionState.Connected or HubConnectionState.Connecting })
            {
                System.Diagnostics.Debug.WriteLine("[OrdersRealtime] StartAsync: already connected or connecting.");
                return;
            }

            if (_apiClient.Http.BaseAddress is null)
            {
                System.Diagnostics.Debug.WriteLine("[OrdersRealtime] StartAsync: missing BaseAddress.");
                ConfigurationError?.Invoke(MissingBaseUrlMessage);
                return;
            }

            var baseUrl = _apiClient.Http.BaseAddress.ToString().TrimEnd('/');
            var hubUrl = $"{baseUrl}/hubs/customer";
            System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] Building connection to {hubUrl}");

            var conn = new HubConnectionBuilder()
                .WithUrl(hubUrl, o =>
                {
                    o.AccessTokenProvider = () =>
                    {
                        var token = _authService.Token;
                        System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] AccessTokenProvider called. HasToken={(string.IsNullOrWhiteSpace(token) ? "false" : "true")}");
                        return Task.FromResult(token);
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            conn.Reconnecting += error =>
            {
                System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] Reconnecting: {error?.Message}");
                return Task.CompletedTask;
            };

            conn.Reconnected += async connectionId =>
            {
                System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] Reconnected. ConnectionId={connectionId}");
                await JoinStaffGroupSafe(conn);
            };

            conn.Closed += error =>
            {
                System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] Closed: {error?.Message}");
                return Task.CompletedTask;
            };

            // Backend: (Guid orderId, string status)
            conn.On<Guid, string>("orderStatusChanged", (orderId, status) =>
            {
                System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] Event orderStatusChanged: {orderId} -> {status}");
                MainThread.BeginInvokeOnMainThread(() => OrderStatusChanged?.Invoke(orderId, status));
            });

            conn.On<ChatMessagePayload>("chatMessage", p =>
            {
                System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] Event chatMessage: table={p.TableId} sender={p.Sender}");
                ChatMessageReceived?.Invoke(p);
            });

            conn.On<PaymentRequestPayload>("ReceivePaymentRequest", p =>
            {
                System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] Event ReceivePaymentRequest: order={p.OrderId} table={p.TableCode}");
                PaymentRequestReceived?.Invoke(p);
            });

            try
            {
                System.Diagnostics.Debug.WriteLine("[OrdersRealtime] Starting connection...");
                await conn.StartAsync();
                System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] Connection started. State={conn.State}, ConnectionId={conn.ConnectionId}");
            }
            catch (Exception startEx)
            {
                System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] StartAsync failed: {startEx}");
                throw;
            }

            await JoinStaffGroupSafe(conn);

            var previous = Interlocked.Exchange(ref _connection, conn);
            if (previous != null)
            {
                System.Diagnostics.Debug.WriteLine("[OrdersRealtime] Disposing previous connection instance.");
                await previous.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] Connection Error: {ex}");
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
            System.Diagnostics.Debug.WriteLine("[OrdersRealtime] Invoking JoinStaffGroup...");
            await conn.InvokeAsync("JoinStaffGroup");
            System.Diagnostics.Debug.WriteLine("[OrdersRealtime] JoinStaffGroup invoked successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrdersRealtime] JoinStaffGroup Failed: {ex}");
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
                System.Diagnostics.Debug.WriteLine("[OrdersRealtime] Stopping and disposing connection.");
                await conn.DisposeAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[OrdersRealtime] StopAsync called but there is no active connection.");
            }
        }
        finally { _startStopLock.Release(); }
    }
}