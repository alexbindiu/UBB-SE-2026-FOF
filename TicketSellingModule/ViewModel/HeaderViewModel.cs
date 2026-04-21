using System.Windows.Input;
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public class HeaderViewModel : ViewModelBase
    {
        public HeaderViewModel()
        {
            NavigateHomeCommand = new RelayCommand(NavigateHome);
        }

        public ICommand NavigateHomeCommand { get; }

        private void NavigateHome()
        {
            NavigationService.Instance?.NavigateToHome();
        }
    }
}
