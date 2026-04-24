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
            services.AddSingleton<DatabaseConnectionFactory>();

            // Repos
            // services.AddTransient<CompanyRepository>();
            // services.AddTransient<AirportRepository>();
            // services.AddTransient<RunwayRepository>();
            // services.AddTransient<GateRepository>();
            // services.AddTransient<EmployeeRepository>();
            // services.AddTransient<FlightRepository>();
            // services.AddTransient<RouteRepository>();
            // services.AddTransient<EmployeeFlightRepository>();
            services.AddSingleton<ICompanyRepository, CompanyRepository>();
            services.AddSingleton<IAirportRepository, AirportRepository>();
            services.AddSingleton<IRunwayRepository, RunwayRepository>();
            services.AddSingleton<IGateRepository, GateRepository>();
            services.AddSingleton<IEmployeeRepository, EmployeeRepository>();
            services.AddSingleton<IFlightRepository, FlightRepository>();
            services.AddSingleton<IRouteRepository, RouteRepository>();
            services.AddSingleton<IEmployeeFlightRepository, EmployeeFlightRepository>();

            // Services
            // services.AddTransient<CompanyService>();
            // services.AddTransient<AirportService>();
            // services.AddTransient<RunwayService>();
            // services.AddTransient<GateService>();
            // services.AddTransient<EmployeeService>();
            // services.AddTransient<FlightRouteService>();
            // services.AddTransient<EmployeeFlightService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ICompanyService, CompanyService>();
            services.AddSingleton<IAirportService, AirportService>();
            services.AddSingleton<IRunwayService, RunwayService>();
            services.AddSingleton<IGateService, GateService>();
            services.AddSingleton<IEmployeeService, EmployeeService>();
            services.AddSingleton<IFlightRouteService, FlightRouteService>();
            services.AddSingleton<IEmployeeFlightService, EmployeeFlightService>();
            services.AddSingleton<IRouteService, RouteService>();

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