using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class HeaderViewModel : ObservableObject
    {
        private readonly INavigationService navigationService;

        public HeaderViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        [RelayCommand]
        private void NavigateHome()
        {
            navigationService.NavigateToHome();
        }
    }
}