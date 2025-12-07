using Microsoft.Maui.Storage;

namespace WaiterApp.Pages;

public partial class SettingsPage : ContentPage
{
    private const string PrefKeyApiBaseUrl = "ApiBaseUrl";

    public SettingsPage()
    {
        InitializeComponent();
        ApiUrlEntry.Text = Preferences.Get(PrefKeyApiBaseUrl, string.Empty);
    }

    private void OnLogout(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }

    private void OnSave(object sender, EventArgs e)
    {
        var url = ApiUrlEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            TestResult.Text = "URL không h?p l?";
            return;
        }
        Preferences.Set(PrefKeyApiBaseUrl, url);
        App.UpdateApiBaseUrl(url);
        TestResult.Text = "?ã l?u c?u hình";
    }

    private async void OnTest(object sender, EventArgs e)
    {
        var url = ApiUrlEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            TestResult.Text = "URL không h?p l?";
            return;
        }
        try
        {
            using var tmp = new HttpClient { BaseAddress = new Uri(url) };
            var resp = await tmp.GetAsync("api/auth/login");
            TestResult.Text = resp.IsSuccessStatusCode ? "K?t n?i OK (endpoint yêu c?u POST)" : $"Ph?n h?i: {(int)resp.StatusCode}";
        }
        catch (Exception ex)
        {
            TestResult.Text = "L?i k?t n?i: " + ex.Message;
        }
    }
}
