using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.AirportAdmin
{
    public sealed partial class AirportDashboardPage : Page
    {
        public AirportDashboardViewModel ViewModel { get; private set; }

        public AirportDashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is AirportDashboardViewModel vm)
            {
                ViewModel = vm;
                DataContext = ViewModel;
                ViewModel.LoadDashboardData();
            }
        }
    }
}