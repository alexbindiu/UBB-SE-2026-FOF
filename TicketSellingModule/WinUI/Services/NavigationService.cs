using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.WinUI;
using TicketSellingModule.WinUI.AirportAdmin;
using TicketSellingModule.WinUI.StaffLogin;

namespace TicketSellingModule.WinUI.Services
{
    public class NavigationService
    {
        public static NavigationService Instance { get; private set; }

        private readonly Frame _frame;

        public NavigationService(Frame frame)
        {
            _frame = frame;
        }

        public static void Initialize(Frame frame)
        {
            Instance = new NavigationService(frame);
        }

        public void NavigateToHome()
        {
            _frame.Navigate(typeof(HomePage));
        }

        public void NavigateToSelectCompany()
        {
            _frame.Navigate(typeof(SelectCompanyPage));
        }

        public void NavigateToCompanyDashboard(int companyId)
        {
            _frame.Navigate(typeof(CompanyPage), companyId);
        }

        public void NavigateToStaffLogin()
        {
            _frame.Navigate(typeof(StaffLoginPage));
        }

        public void NavigateToAirportAdmin()
        {
            _frame.Navigate(typeof(AirportAdminPage));
        }
    }
}
