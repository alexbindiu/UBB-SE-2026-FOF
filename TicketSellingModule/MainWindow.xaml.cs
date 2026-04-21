using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using TicketSellingModule.WinUI;
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            var navigationService = App.Services.GetRequiredService<INavigationService>();
            navigationService.Initialize(RootFrame);

            RootFrame.Navigate(typeof(HomePage), 1);
        }
    }
}