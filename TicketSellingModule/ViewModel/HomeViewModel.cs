using System.Windows.Input;
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public class HomeViewModel : ViewModelBase
    {
        public HomeViewModel()
        {
            NavigateToCompanyCommand = new RelayCommand(NavigateToCompany);
            NavigateToAdminCommand = new RelayCommand(NavigateToAdmin);
            NavigateToStaffCommand = new RelayCommand(NavigateToStaff);
        }

        public ICommand NavigateToCompanyCommand { get; }
        public ICommand NavigateToAdminCommand { get; }
        public ICommand NavigateToStaffCommand { get; }

        private void NavigateToCompany()
        {
            NavigationService.Instance?.NavigateToSelectCompany();
        }

        private void NavigateToAdmin()
        {
            NavigationService.Instance?.NavigateToAirportAdmin();
        }

        private void NavigateToStaff()
        {
            NavigationService.Instance?.NavigateToStaffLogin();
        }
    }
}
