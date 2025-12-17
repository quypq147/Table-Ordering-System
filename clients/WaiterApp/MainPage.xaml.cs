using WaiterApp.Services;

namespace WaiterApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Đảm bảo kết nối SignalR cho Orders (CustomerHub)
        _ = App.OrdersRealtimeService.StartAsync();

        // Đảm bảo kết nối SignalR cho KDS (KdsHub)
        _ = App.KdsRealtimeService.StartAsync();
    }
}
