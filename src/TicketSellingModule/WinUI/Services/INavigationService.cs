using Microsoft.UI.Xaml.Controls;

namespace TicketSellingModule.WinUI.Services
{
    public interface INavigationService
    {
        void Initialize(Frame frame);
        void NavigateToHome();
        void NavigateToSelectCompany();
        void NavigateToCompanyDashboard(int companyId);
        void NavigateToStaffLogin();
        void NavigateToAirportAdmin();
        void NavigateToStaffDashboard(int employeeId);
    }
}