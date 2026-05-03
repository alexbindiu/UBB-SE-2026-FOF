using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

using TicketSellingModule.ViewModel;
using TicketSellingModule.WinUI.AirportAdmin;
using TicketSellingModule.WinUI.StaffLogin;

namespace TicketSellingModule.WinUI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider services;
        private Frame frame;

        public NavigationService(IServiceProvider services)
        {
            this.services = services;
        }

        public void Initialize(Frame frame)
        {
            this.frame = frame;
        }

        public void NavigateToHome()
        {
            frame.Navigate(typeof(HomePage), services.GetRequiredService<HomeViewModel>());
        }

        public void NavigateToSelectCompany()
        {
            frame.Navigate(typeof(SelectCompanyPage), services.GetRequiredService<SelectCompanyViewModel>());
        }

        public void NavigateToCompanyDashboard(int companyId)
        {
            frame.Navigate(typeof(CompanyPage), (services.GetRequiredService<CompanyViewModel>(), companyId));
        }

        public void NavigateToStaffLogin()
        {
            frame.Navigate(typeof(StaffLoginPage), services.GetRequiredService<StaffLoginViewModel>());
        }

        public void NavigateToAirportAdmin()
        {
            frame.Navigate(
                typeof(AirportAdminPage),
                (
                    services.GetRequiredService<AirportAdminViewModel>(),
                    services.GetRequiredService<FlightsDashboardViewModel>(),
                    services.GetRequiredService<EmployeesDashboardViewModel>(),
                    services.GetRequiredService<AirportDashboardViewModel>()));
        }

        public void NavigateToStaffDashboard(int employeeId)
        {
            frame.Navigate(typeof(StaffPage), (services.GetRequiredService<StaffPageViewModel>(), employeeId));
        }
    }
}
