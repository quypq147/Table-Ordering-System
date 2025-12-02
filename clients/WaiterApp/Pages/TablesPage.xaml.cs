using System.Net.Http.Json;
using WaiterApp.Models;
using WaiterApp;

namespace WaiterApp.Pages;

public partial class TablesPage : ContentPage
{
    public TablesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTablesAsync();
    }

    private async Task LoadTablesAsync()
    {
        try
        {
            var tables = await App.ApiClient.Http.GetFromJsonAsync<List<TableDto>>(WaiterApiEndpoints.Tables.List);
            TablesCollectionView.ItemsSource = tables;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi", ex.Message, "OK");
        }
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await LoadTablesAsync();
    }

    private async void OnTableSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is TableDto table)
        {
            await Shell.Current.GoToAsync($"tabledetail?tableId={table.Id}");
        }
    }
}
