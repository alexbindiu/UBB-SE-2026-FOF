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
            this.navigationService = navigationService;
        }

        [RelayCommand]
        private void NavigateToCompany()
        {
            this.navigationService.NavigateToSelectCompany();
        }

        [RelayCommand]
        private void NavigateToAdmin()
        {
            this.navigationService.NavigateToAirportAdmin();
        }

        [RelayCommand]
        private void NavigateToStaff()
        {
            this.navigationService.NavigateToStaffLogin();
        }
    }
}