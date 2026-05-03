using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.Input;

using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class HomeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
