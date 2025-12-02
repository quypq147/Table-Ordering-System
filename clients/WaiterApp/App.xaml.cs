using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;
using WaiterApp.Services;

namespace WaiterApp
{
    public partial class App : Application
    {
        // Create ApiClient using a platform-aware base address so emulators/devices can reach the backend
        public static ApiClient ApiClient { get; } =
            new ApiClient(GetApiBaseAddress());

        // Create instance AuthService using the shared ApiClient
        public static AuthService AuthService { get; } = new AuthService(ApiClient);

        // Realtime services singletons
        public static OrdersRealtimeService OrdersRealtimeService { get; } = new OrdersRealtimeService(AuthService, ApiClient);
        public static KdsRealtimeService KdsRealtimeService { get; } = new KdsRealtimeService(AuthService, ApiClient);

        private static string GetApiBaseAddress()
        {
            // Default port used by backend during development
            const int port = 5075;

            // Android emulator (Google) uses 10.0.2.2 to reach host machine
            if (DeviceInfo.Platform == DevicePlatform.Android)
                return $"http://10.0.2.2:{port}";

            // Android emulator (Android Emulator from Visual Studio / Hyper-V) sometimes uses 10.0.2.2 as well.
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

            MainPage = new NavigationPage(new Pages.LoginPage());
        }
    }
}
