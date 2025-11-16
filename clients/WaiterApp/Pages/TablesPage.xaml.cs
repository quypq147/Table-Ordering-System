using System.Net.Http.Json;
using WaiterApp.Models;
using WaiterApp.Services;

namespace WaiterApp.Pages;

public partial class TablesPage : ContentPage
{
    private readonly ApiClient _apiClient;

    public TablesPage()
    {
        InitializeComponent();
        _apiClient = App.ApiClient;
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
            var tables = await _apiClient.Http
                .GetFromJsonAsync<List<TableDto>>("api/tables");

            TablesCollection.ItemsSource = tables;
        }
        catch (Exception ex)
        {
            await DisplayAlert("L?i", ex.Message, "OK");
        }
    }

    private async void OnTableSelected(object sender, SelectionChangedEventArgs e)
    {
        var table = e.CurrentSelection.FirstOrDefault() as TableDto;
        if (table == null) return;

        await Navigation.PushAsync(new TableDetailPage(table));
        ((CollectionView)sender).SelectedItem = null;
    }
}
