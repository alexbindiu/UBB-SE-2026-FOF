using Microsoft.UI.Xaml;
using TicketSellingModule.WinUI; // Brings in your WinUI folder!
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            NavigationService.Initialize(RootFrame);
            RootFrame.Navigate(typeof(HomePage), 1);
        }
    }
}