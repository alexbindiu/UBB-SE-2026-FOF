using Microsoft.UI.Xaml;
using TicketSellingModule.WinUI; // Brings in your WinUI folder!

namespace TicketSellingModule
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            RootFrame.Navigate(typeof(SelectCompanyPage));
        }
    }
}