using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.AirportAdmin
{
    public sealed partial class AirportAdminPage : Page
    {
        public AirportAdminViewModel ViewModel { get; }
        private readonly EmployeesDashboardViewModel _employeesViewModel;
        private readonly AirportDashboardViewModel _airportViewModel;

        public AirportAdminPage()
        {
            this.InitializeComponent();
            
            _airportViewModel = new AirportDashboardViewModel(new AirportService(new AirportRepo(connectionFactory)),
                                                                new RunwayService(new RunwayRepo(connectionFactory)),
                                                                new GateService(new GateRepo(connectionFactory)));

            Loaded += AirportAdminPage_Loaded;
            ViewModel = App.Services.GetRequiredService<AirportAdminViewModel>();
            _employeesViewModel = App.Services.GetRequiredService<EmployeesDashboardViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.Initialize();
            ContentFrame.Navigate(typeof(FlightsDashboardPage), ViewModel);
            HighlightSelectedButton(FlightsButton);
        }

        private void FlightsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(FlightsDashboardPage), ViewModel);
            HighlightSelectedButton(FlightsButton);
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(EmployeesDashboardPage), _employeesViewModel);
            HighlightSelectedButton(EmployeesButton);
        }

        private void AirportButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(AirportDashboardPage), _airportViewModel);
            HighlightSelectedButton(AirportButton);
        }

        private void HighlightSelectedButton(Button selectedButton)
        {
            FlightsButton.Background = null;
            EmployeesButton.Background = null;
            AirportButton.Background = null;

            selectedButton.Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray);
        }
    }
}
