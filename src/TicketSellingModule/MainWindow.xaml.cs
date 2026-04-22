using Microsoft.UI.Xaml;
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow(INavigationService navigationService)
        {
            this.InitializeComponent();

            navigationService.Initialize(RootFrame);
            navigationService.NavigateToHome();
        }
    }
}