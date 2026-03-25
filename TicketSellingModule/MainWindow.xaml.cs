using Microsoft.UI.Xaml;
using TicketSellingModule.WinUI; // Brings in your WinUI folder!

namespace TicketSellingModule
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // Tell the Frame to navigate to your new dashboard
            RootFrame.Navigate(typeof(CompanyPage));
        }
    }
}