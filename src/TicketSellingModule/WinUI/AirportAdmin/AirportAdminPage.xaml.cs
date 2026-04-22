using System.ComponentModel;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.AirportAdmin
{
    public sealed partial class AirportAdminPage : Page
    {
        public AirportAdminViewModel ViewModel { get; private set; } = null!;
        private FlightsDashboardViewModel flightsViewModel = null!;
        private EmployeesDashboardViewModel employeesViewModel = null!;
        private AirportDashboardViewModel airportViewModel = null!;

        public AirportAdminPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<AirportAdminViewModel, FlightsDashboardViewModel, EmployeesDashboardViewModel, AirportDashboardViewModel> context)
            {
                ViewModel = context.Item1;
                flightsViewModel = context.Item2;
                employeesViewModel = context.Item3;
                airportViewModel = context.Item4;

                DataContext = ViewModel;
                Bindings.Update();

                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
                ViewModel.Initialize();
                NavigateToSelectedSection();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            base.OnNavigatedFrom(e);
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AirportAdminViewModel.SelectedSection))
            {
                NavigateToSelectedSection();
            }
        }

        private void NavigateToSelectedSection()
        {
            switch (ViewModel.SelectedSection)
            {
                case AirportAdminSection.Flights:
                    if (ContentFrame.CurrentSourcePageType != typeof(FlightsDashboardPage))
                    {
                        ContentFrame.Navigate(typeof(FlightsDashboardPage), flightsViewModel);
                    }
                    HighlightSelectedButton(FlightsButton);
                    break;

                case AirportAdminSection.Employees:
                    if (ContentFrame.CurrentSourcePageType != typeof(EmployeesDashboardPage))
                    {
                        ContentFrame.Navigate(typeof(EmployeesDashboardPage), employeesViewModel);
                    }

                    HighlightSelectedButton(EmployeesButton);
                    break;

                case AirportAdminSection.Airport:
                    if (ContentFrame.CurrentSourcePageType != typeof(AirportDashboardPage))
                    {
                        ContentFrame.Navigate(typeof(AirportDashboardPage), airportViewModel);
                    }

                    HighlightSelectedButton(AirportButton);
                    break;
            }
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
