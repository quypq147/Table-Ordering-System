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
            ErrorLabel.Text = "Vui lòng nhập đầy đủ tài khoản và mật khẩu.";
            ErrorLabel.IsVisible = true;
            return;
        }

        // Call LoginAsync which returns a tuple (bool Success, string? Error)
        var (success, error) = await _authService.LoginAsync(username, password);

        if (!success)
        {

            ErrorLabel.Text = string.IsNullOrWhiteSpace(error) ? "Đăng nhập thất bại." : error;
            ErrorLabel.IsVisible = true;
            return;
        }

        Application.Current!.MainPage = new AppShell();
        await Shell.Current.GoToAsync("orders");
    }
}
