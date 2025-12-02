namespace WaiterApp.Pages;

using WaiterApp.Services;
using WaiterApp;
using System.Net.Http.Json;
using System.Linq;

public partial class OrdersPage : ContentPage
{
    private enum OrderFilter
    {
        New,
        InProgress,
        Completed
    }

    private readonly ApiClient _apiClient = App.ApiClient;
    private readonly OrdersRealtimeService _realtime = App.OrdersRealtimeService;
    private List<OrderSummaryVm> _orders = new();
    private OrderFilter _activeFilter = OrderFilter.New;
    private CancellationTokenSource? _cts;

    public OrdersPage() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _cts = new CancellationTokenSource();
        _realtime.OrderStatusChanged += OnOrderStatusChanged;
        await _realtime.StartAsync();
        await LoadAsync();
        UpdateFilterButtonsVisualState();
        ApplyFilters();
        _ = AutoRefreshAsync(_cts.Token);
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        _realtime.OrderStatusChanged -= OnOrderStatusChanged;
        _cts?.Cancel();
        await _realtime.StopAsync();
    }

    private void OnOrderStatusChanged(Guid orderId, string status)
    {
        MainThread.BeginInvokeOnMainThread(async () => { await LoadAsync(); ApplyFilters(); });
    }

    private async Task AutoRefreshAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), ct);
                if (ct.IsCancellationRequested) break;
                await LoadAsync();
                ApplyFilters();
            }
        }
        catch (TaskCanceledException) { }
    }

    private async Task LoadAsync()
    {
        try
        {
            OrdersRefresh.IsRefreshing = true;

            var url = WaiterApiEndpoints.Orders.Summaries(page: 1, pageSize: 200);
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var client = _apiClient.Http;
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                await DisplayAlertAsync("API Error",
                    $"Status: {(int)response.StatusCode} {response.ReasonPhrase}\n\n{content}", "OK");
                return;
            }

            var data = await response.Content.ReadFromJsonAsync<List<OrderSummaryVm>>();
            _orders = data ?? new();
        }
        finally
        {
            OrdersRefresh.IsRefreshing = false;
        }
    }

    private void ApplyFilters()
    {
        IEnumerable<OrderSummaryVm> query = _orders;

        var term = SearchBar?.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var low = term.ToLowerInvariant();
            query = query.Where(o =>
                (!string.IsNullOrEmpty(o.TableCode) && o.TableCode.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                // some projects may have OrderNumber or Code field
                (o.GetType().GetProperty("OrderNumber")?.GetValue(o) is string on && on.Contains(term, StringComparison.OrdinalIgnoreCase))
            );
        }

        string[] statusFilter = Array.Empty<string>();

        switch (_activeFilter)
        {
            case OrderFilter.New:
                statusFilter = new[] { "Submitted" };
                break;
            case OrderFilter.InProgress:
                statusFilter = new[] { "InProgress", "Ready", "WaitingForPayment" };
                break;
            case OrderFilter.Completed:
                statusFilter = new[] { "Paid", "Cancelled" };
                break;
        }

        if (statusFilter.Length > 0)
            query = query.Where(o => statusFilter.Contains(o.Status));

        OrdersCollection.ItemsSource = query.ToList();
    }

    private void OnFilterClicked(object sender, EventArgs e)
    {
        if (sender is Button b)
        {
            _activeFilter = b.Text switch
            {
                "Đang làm" => OrderFilter.InProgress,
                "Hoàn tất" => OrderFilter.Completed,
                "Mới" => OrderFilter.New,
                "InProgress" => OrderFilter.InProgress,
                "Completed" => OrderFilter.Completed,
                "New" => OrderFilter.New,
                _ => _activeFilter
            };

            UpdateFilterButtonsVisualState();
            ApplyFilters();
        }
    }

    private void UpdateFilterButtonsVisualState()
    {
        // try to find buttons by name if available
        try
        {
            if (this.FindByName<Button>("BtnPending") is Button btnPending)
            {
                btnPending.BackgroundColor = Colors.Transparent;
                btnPending.TextColor = Colors.Black;
            }

            if (this.FindByName<Button>("BtnPreparing") is Button btnPreparing)
            {
                btnPreparing.BackgroundColor = Colors.Transparent;
                btnPreparing.TextColor = Colors.Black;
            }

            if (this.FindByName<Button>("BtnCompleted") is Button btnCompleted)
            {
                btnCompleted.BackgroundColor = Colors.Transparent;
                btnCompleted.TextColor = Colors.Black;
            }

            var activeColor = Color.FromArgb("#2563EB");

            switch (_activeFilter)
            {
                case OrderFilter.New:
                    if (this.FindByName<Button>("BtnPending") is Button bp)
                    {
                        bp.BackgroundColor = activeColor;
                        bp.TextColor = Colors.White;
                    }
                    break;
                case OrderFilter.InProgress:
                    if (this.FindByName<Button>("BtnPreparing") is Button bpr)
                    {
                        bpr.BackgroundColor = activeColor;
                        bpr.TextColor = Colors.White;
                    }
                    break;
                case OrderFilter.Completed:
                    if (this.FindByName<Button>("BtnCompleted") is Button bc)
                    {
                        bc.BackgroundColor = activeColor;
                        bc.TextColor = Colors.White;
                    }
                    break;
            }
        }
        catch
        {
            // ignore visual update failures on platforms where Colors/FindByName differ
        }
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

    private async void OnOrderSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not OrderSummaryVm order) return;
        await Shell.Current.GoToAsync($"orderdetail?orderId={order.Id}");
        ((CollectionView)sender).SelectedItem = null;
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await LoadAsync();
        ApplyFilters();
    }
}

public sealed class OrderSummaryVm
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }
    public string TableCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int PendingItems { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
