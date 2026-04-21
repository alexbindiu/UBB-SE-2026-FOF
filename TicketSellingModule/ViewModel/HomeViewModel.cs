using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        public HomeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void NavigateToCompany()
        {
            _navigationService.NavigateToSelectCompany();
        }

        [RelayCommand]
        private void NavigateToAdmin()
        {
            _navigationService.NavigateToAirportAdmin();
        }

        [RelayCommand]
        private void NavigateToStaff()
        {
            _navigationService.NavigateToStaffLogin();
        }
    }
}