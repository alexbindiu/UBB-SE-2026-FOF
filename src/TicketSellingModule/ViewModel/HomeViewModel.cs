using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly INavigationService navigationService;

        public HomeViewModel(INavigationService navigationService)
        {
            navigationService = navigationService;
        }

        [RelayCommand]
        private void NavigateToCompany()
        {
            navigationService.NavigateToSelectCompany();
        }

        [RelayCommand]
        private void NavigateToAdmin()
        {
            navigationService.NavigateToAirportAdmin();
        }

        [RelayCommand]
        private void NavigateToStaff()
        {
            navigationService.NavigateToStaffLogin();
        }
    }
}