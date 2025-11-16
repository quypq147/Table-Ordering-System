using WaiterApp.Services;

namespace WaiterApp
{
    public partial class App : Application
    {
        public static ApiClient ApiClient { get; } =
            new ApiClient("https://your-backend-base-url"); // TODO: change base url

        public static AuthService AuthService { get; } = new(AuthServiceClient: ApiClient);
        public static KdsRealtimeService KdsRealtimeService { get; } = new(AuthService, ApiClient);

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new Pages.LoginPage());
        }
    }
}
