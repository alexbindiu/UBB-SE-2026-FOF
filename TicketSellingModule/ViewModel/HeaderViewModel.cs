using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class HeaderViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        public HeaderViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void NavigateHome()
        {
            _navigationService.NavigateToHome();
        }
    }
}