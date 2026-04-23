using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using TicketSellingModule.ViewModel;
using TicketSellingModule.WinUI.Services;
namespace TicketSellingModule
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        private Window window;

        public App()
        {
            InitializeComponent();

            try
            {
                var services = new ServiceCollection();
                ConfigureServices(services);
                Services = services.BuildServiceProvider();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DI ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"INNER: {ex.InnerException?.Message}");
                throw;
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // Infrastructure
            services.AddSingleton<DbConnectionFactory>();

            // Repos
            services.AddTransient<CompanyRepo>();
            services.AddTransient<AirportRepo>();
            services.AddTransient<RunwayRepo>();
            services.AddTransient<GateRepo>();
            services.AddTransient<EmployeeRepo>();
            services.AddTransient<FlightRepo>();
            services.AddTransient<RouteRepo>();
            services.AddTransient<EmployeeFlightRepo>();

            // Services
            services.AddTransient<CompanyService>();
            services.AddTransient<AirportService>();
            services.AddTransient<RunwayService>();
            services.AddTransient<GateService>();
            services.AddTransient<EmployeeService>();
            services.AddTransient<FlightRouteService>();
            services.AddTransient<EmployeeFlightService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // ViewModels
            services.AddTransient<SelectCompanyViewModel>();
            services.AddTransient<AirportAdminViewModel>();
            services.AddTransient<EmployeesDashboardViewModel>();
            services.AddTransient<AirportDashboardViewModel>();
            services.AddTransient<FlightsDashboardViewModel>();
            services.AddTransient<RouteRepo>();
            services.AddTransient<RouteService>();
            services.AddTransient<CompanyViewModel>();
            services.AddTransient<StaffPageViewModel>();
            services.AddTransient<HeaderViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<StaffLoginViewModel>();

            // Shell
            services.AddSingleton<MainWindow>();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                window = Services.GetRequiredService<MainWindow>();
                window.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LAUNCH ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"INNER: {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}