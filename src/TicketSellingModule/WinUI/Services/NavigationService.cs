using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

using TicketSellingModule.ViewModel;
using TicketSellingModule.WinUI;
using TicketSellingModule.WinUI.AirportAdmin;
using TicketSellingModule.WinUI.StaffLogin;

namespace TicketSellingModule.WinUI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _services;
        private Frame _frame;

        public NavigationService(IServiceProvider services)
        {
            _services = services;
        }

        public void Initialize(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateToHome() =>
            _frame.Navigate(typeof(HomePage), _services.GetRequiredService<HomeViewModel>());

        public void NavigateToSelectCompany() =>
            _frame.Navigate(typeof(SelectCompanyPage), _services.GetRequiredService<SelectCompanyViewModel>());

        public void NavigateToCompanyDashboard(int companyId) =>
            _frame.Navigate(typeof(CompanyPage), (_services.GetRequiredService<CompanyViewModel>(), companyId));

        public void NavigateToStaffLogin() =>
            _frame.Navigate(typeof(StaffLoginPage), _services.GetRequiredService<StaffLoginViewModel>());

        public void NavigateToAirportAdmin() =>
            _frame.Navigate(
                typeof(AirportAdminPage),
                (
                    _services.GetRequiredService<AirportAdminViewModel>(),
                    _services.GetRequiredService<FlightsDashboardViewModel>(),
                    _services.GetRequiredService<EmployeesDashboardViewModel>(),
                    _services.GetRequiredService<AirportDashboardViewModel>()
                ));

        public void NavigateToStaffDashboard(int employeeId) =>
            _frame.Navigate(typeof(StaffPage), (_services.GetRequiredService<StaffPageViewModel>(), employeeId));
    }
}
