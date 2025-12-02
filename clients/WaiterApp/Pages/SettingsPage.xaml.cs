namespace WaiterApp.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void OnLogout(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }
}
