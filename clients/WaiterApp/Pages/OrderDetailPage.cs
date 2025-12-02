namespace WaiterApp.Pages;

using WaiterApp;
using WaiterApp.Services;
using System.Net.Http.Json;

public sealed class OrderDetailPage : ContentPage
{
    private readonly ApiClient _apiClient = App.ApiClient;
    private readonly Guid _orderId;

    private readonly Label _header = new() { FontSize = 20, FontAttributes = FontAttributes.Bold };
    private readonly Label _status = new() { FontSize = 14, TextColor = Colors.Gray };
    private readonly CollectionView _items = new();
    private readonly Button _btnInProgress = new() { Text = "Bắt đầu nấu" };
    private readonly Button _btnReady = new() { Text = "Đánh dấu đã sẵn sàng" };
    private readonly Button _btnServed = new() { Text = "Đánh dấu đã phục vụ" };

    private OrderDtoVm? _order;

    public OrderDetailPage(Guid orderId)
    {
        _orderId = orderId;
        Title = "ĐƠN HÀNG";

        _items.ItemTemplate = new DataTemplate(() =>
        {
            var name = new Label { FontAttributes = FontAttributes.Bold };
            name.SetBinding(Label.TextProperty, nameof(OrderItemVm.Name));
            var note = new Label { FontSize = 12, TextColor = Colors.Gray };
            note.SetBinding(Label.TextProperty, nameof(OrderItemVm.Note));

            var qty = new Label();
            qty.SetBinding(Label.TextProperty, nameof(OrderItemVm.Quantity));
            var line = new Label { FontSize = 12 };
            line.SetBinding(Label.TextProperty, nameof(OrderItemVm.LineTotal));

            var left = new VerticalStackLayout { Children = { name, note } };
            var right = new VerticalStackLayout { HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center, Children = { qty, line } };
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Padding = 4 };
            grid.Add(left, 0, 0);
            grid.Add(right, 1, 0);
            return grid;
        });

        _btnInProgress.Clicked += async (_, __) => await StartOrderAsync();
        _btnReady.Clicked += async (_, __) => await CallAsync(WaiterApiEndpoints.Orders.InProgress(_orderId));
        _btnServed.Clicked += async (_, __) => await CallAsync(WaiterApiEndpoints.Orders.Served(_orderId));

        var buttons = new HorizontalStackLayout { Spacing = 12, Children = { _btnServed, _btnReady, _btnInProgress } };

        var grid = new Grid
        {
            Padding = 16,
            BackgroundColor = Color.FromArgb("#F5F6F8"),
            RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) }
        };
        grid.Add(_header);
        grid.Add(_status, 0, 1);
        grid.Add(_items, 0, 2);
        grid.Add(buttons, 0, 3);

        Content = grid;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var dto = await _apiClient.Http
                .GetFromJsonAsync<OrderDtoVm>(WaiterApiEndpoints.Orders.Get(_orderId));

            if (dto == null) return;
            _order = dto;

            _header.Text = $"Đơn #{_orderId.ToString()[..8]}";
            _status.Text = StatusText(dto.Status);
            _items.ItemsSource = dto.Items;
            UpdateButtons();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi", ex.Message, "OK");
        }
    }

    private async Task StartOrderAsync()
    {
        if (_order == null)
        {
            await DisplayAlertAsync("Lỗi", "Không tìm thấy thông tin đơn hàng.", "OK");
            return;
        }

        try
        {
            var body = new
            {
                OrderId = _order.Id,
                TableId = _order.TableId
            };

            var resp = await _apiClient.Http.PostAsJsonAsync(WaiterApiEndpoints.Orders.Start, body);
            if (resp.IsSuccessStatusCode)
            {
                await LoadAsync();
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                await DisplayAlertAsync("Lỗi", error, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi", ex.Message, "OK");
        }
    }

    private async Task CallAsync(string path)
    {
        try
        {
            var resp = await _apiClient.Http.PostAsync(path, null);
            if (resp.IsSuccessStatusCode) await LoadAsync();
            else await DisplayAlertAsync("Lỗi", await resp.Content.ReadAsStringAsync(), "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi", ex.Message, "OK");
        }
    }

    private void UpdateButtons()
    {
        if (_order == null) return;
        _btnInProgress.IsVisible = _order.Status == 1;   // Submitted
        _btnReady.IsVisible = _order.Status == 2;        // InProgress
        _btnServed.IsVisible = _order.Status == 3;       // Ready
    }

    private string StatusText(int status)
    {
        return status switch
        {
            0 => "New",
            1 => "Submitted",
            2 => "In Progress",
            3 => "Ready",
            4 => "Served",
            5 => "Cancelled",
            _ => "Unknown"
        };
    }
}

public sealed class OrderDtoVm
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }
    public int Status { get; set; }
    public List<OrderItemVm> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public sealed class OrderItemVm
{
    public int OrderItemId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public string? Note { get; set; }
}
