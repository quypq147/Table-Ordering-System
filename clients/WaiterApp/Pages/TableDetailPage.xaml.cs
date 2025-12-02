using WaiterApp;
using System.Net.Http.Json;
using WaiterApp.Models;
using WaiterApp.Services;

namespace WaiterApp.Pages;

public partial class TableDetailPage : ContentPage
{
    private readonly ApiClient _apiClient;
    private readonly TableDto _table;

    public TableDetailPage(TableDto table)
    {
        InitializeComponent();
        _apiClient = App.ApiClient;
        _table = table;

        Title = $"Bàn {_table.Code}";
        TableCodeLabel.Text = _table.Code;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadOrderAsync();
    }

    private async Task LoadOrderAsync()
    {
        try
        {
            var items = await _apiClient.Http
                .GetFromJsonAsync<List<OrderItemDto>>(
                    WaiterApiEndpoints.Orders.ItemsByTable(_table.Id));

            OrderItemsCollection.ItemsSource = items;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Loi", ex.Message, "OK");
        }
    }

    private async void OnViewTicketsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TicketsPage(_table));
    }
}
