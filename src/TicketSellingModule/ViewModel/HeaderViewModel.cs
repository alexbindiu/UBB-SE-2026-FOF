using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.Input;

using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class HeaderViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
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

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}