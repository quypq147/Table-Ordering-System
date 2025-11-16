using WaiterApp.Services;

namespace WaiterApp.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;

    public LoginPage()
    {
        InitializeComponent();
        _authService = App.AuthService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            ErrorLabel.Text = "Vui lòng nh?p ??y ?? tài kho?n và m?t kh?u.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var success = await _authService.LoginAsync(username, password);

        if (!success)
        {
            ErrorLabel.Text = "??ng nh?p th?t b?i.";
            ErrorLabel.IsVisible = true;
            return;
        }

        await Navigation.PushAsync(new TablesPage());
    }
}
