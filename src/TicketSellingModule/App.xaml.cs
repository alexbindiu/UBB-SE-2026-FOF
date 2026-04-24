using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using TicketSellingModule.Data.Repositories.Interfaces;
using TicketSellingModule.Data.Services.Interfaces;
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
            services.AddTransient<DatabaseConnectionFactory>();

            // Repos
            services.AddTransient<ICompanyRepository, CompanyRepository>();
            services.AddTransient<IAirportRepository, AirportRepository>();
            services.AddTransient<IRunwayRepository, RunwayRepository>();
            services.AddTransient<IGateRepository, GateRepository>();
            services.AddTransient<IEmployeeRepository, EmployeeRepository>();
            services.AddTransient<IFlightRepository, FlightRepository>();
            services.AddTransient<IRouteRepository, RouteRepository>();
            services.AddTransient<IEmployeeFlightRepository, EmployeeFlightRepository>();

            // Services
            services.AddTransient<INavigationService, NavigationService>();
            services.AddTransient<ICompanyService, CompanyService>();
            services.AddTransient<IAirportService, AirportService>();
            services.AddTransient<IRunwayService, RunwayService>();
            services.AddTransient<IGateService, GateService>();
            services.AddTransient<IEmployeeService, EmployeeService>();
            services.AddTransient<IFlightRouteService, FlightRouteService>();
            services.AddTransient<IEmployeeFlightService, EmployeeFlightService>();
            services.AddTransient<IRouteService, RouteService>();

            // ViewModels
            services.AddTransient<SelectCompanyViewModel>();
            services.AddTransient<AirportAdminViewModel>();
            services.AddTransient<EmployeesDashboardViewModel>();
            services.AddTransient<AirportDashboardViewModel>();
            services.AddTransient<FlightsDashboardViewModel>();
            services.AddTransient<RouteRepository>();
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