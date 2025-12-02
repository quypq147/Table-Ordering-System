using Microsoft.Maui.Controls;
using WaiterApp.Pages;
using WaiterApp.Services;

namespace WaiterApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("orders", typeof(Pages.OrdersPage));
        Routing.RegisterRoute("tables", typeof(TablesPage));

        // Register routes for detail pages
        Routing.RegisterRoute("tabledetail", typeof(Pages.TableDetailPage));
        Routing.RegisterRoute("orderdetail", typeof(Pages.OrderDetailPage));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await SecureStorage.SetAsync("jwt_token", string.Empty);
        await Current.Navigation.PopToRootAsync();
        Application.Current!.MainPage = new AppShell();
    }
}
