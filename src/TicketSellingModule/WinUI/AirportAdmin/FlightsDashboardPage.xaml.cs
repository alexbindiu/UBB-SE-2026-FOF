using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.AirportAdmin
{
    public sealed partial class FlightsDashboardPage : Page
    {
        public FlightsDashboardViewModel ViewModel { get; private set; }

        public FlightsDashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is FlightsDashboardViewModel vm)
            {
                ViewModel = vm;
                DataContext = ViewModel;
                ViewModel.LoadFlights();
            }
        }
    }
}