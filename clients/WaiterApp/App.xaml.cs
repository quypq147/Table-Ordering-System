using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using WaiterApp.Services;

namespace WaiterApp
{
    public partial class App : Application
    {
        private const string PrefKeyApiBaseUrl = "ApiBaseUrl";

        // Create ApiClient using a platform-aware base address so emulators/devices can reach the backend
        public static ApiClient ApiClient { get; } =
            new ApiClient(GetInitialApiBaseAddress());

        // Create instance AuthService using the shared ApiClient
        public static AuthService AuthService { get; } = new AuthService(ApiClient);

        // Realtime services singletons
        public static OrdersRealtimeService OrdersRealtimeService { get; } = new OrdersRealtimeService(AuthService, ApiClient);
        public static KdsRealtimeService KdsRealtimeService { get; } = new KdsRealtimeService(AuthService, ApiClient);

        private static string GetInitialApiBaseAddress()
        {
            var saved = Preferences.Get(PrefKeyApiBaseUrl, null);
            if (!string.IsNullOrWhiteSpace(saved))
                return saved!;

            // Default port used by backend during development
            const int port = 5075;

            // Android emulator (Google) uses 10.0.2.2 to reach host machine
            if (DeviceInfo.Platform == DevicePlatform.Android)
                return $"http://10.0.2.2:{port}";

            // iOS simulator and Windows can use localhost
            if (DeviceInfo.Platform == DevicePlatform.iOS ||
                DeviceInfo.Platform == DevicePlatform.MacCatalyst ||
                DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                return $"http://localhost:{port}";
            }

            // Fallback: use localhost
            return $"http://localhost:{port}";
        }

        public App()
        {
            InitializeComponent();

            // Subscribe friendly configuration errors to navigate to settings
            OrdersRealtimeService.ConfigurationError += OnConfigurationError;
            KdsRealtimeService.ConfigurationError += OnConfigurationError;

            MainPage = new NavigationPage(new Pages.LoginPage());
        }

        private async void OnConfigurationError(string message)
        {
            await MainPage.DisplayAlert("Cấu hình thiếu", message, "Thiết lập");
            await MainPage.Navigation.PushAsync(new Pages.SettingsPage());
        }

        public static void UpdateApiBaseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            Preferences.Set(PrefKeyApiBaseUrl, url);
            ApiClient.SetBaseAddress(url);
        }
    }
}
