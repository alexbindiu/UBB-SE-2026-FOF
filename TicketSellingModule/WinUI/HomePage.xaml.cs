using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            DataContext = new HomeViewModel();
        }
    }
}
