using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

namespace TicketSellingModule.ViewModel
{
    public partial class CompanyViewModel(
        ICompanyService companyService,
        IAirportService airportService,
        IFlightRouteService flightRouteService,
        IRunwayService runwayService,
        IGateService gateService,
        IEmployeeFlightService employeeFlightService) : ObservableObject
    {
        private const string CustomRecurrenceType = "Custom";
        private const int DefaultEndRecurrenceInterval = 7;
        private const int DefaultDepartureHour = 12;
        private const int DefaultArrivalHour = 13;
        private const int DefaultMinute = 0;
        private const int DefaultIdInCaseOfNull = 0;

        private int currentCompanyId;
        private List<Flight> masterFlightsCollection = new();

        [ObservableProperty] private ObservableCollection<Company> companiesList;
        [ObservableProperty] private ObservableCollection<Airport> airportsList;
        [ObservableProperty] private ObservableCollection<Flight> companyFlightsList;
        [ObservableProperty] private ObservableCollection<Runway> runwaysList;
        [ObservableProperty] private ObservableCollection<Gate> gatesList;

        [ObservableProperty] private string flightNumberSearchQuery = string.Empty;
        [ObservableProperty] private string? selectedRouteType;
        [ObservableProperty] private Airport? selectedAirport;
        [ObservableProperty] private string capacityText = string.Empty;
        [ObservableProperty] private TimeSpan departureTime = TimeSpan.Zero;
        [ObservableProperty] private TimeSpan arrivalTime = TimeSpan.Zero;
        [ObservableProperty] private DateTimeOffset? singleDate;
        [ObservableProperty] private string customDaysText = string.Empty;
        [ObservableProperty] private DateTimeOffset? startDate;
        [ObservableProperty] private DateTimeOffset? endDate;
        [ObservableProperty] private Runway? selectedRunway;
        [ObservableProperty] private Gate? selectedGate;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RecurrentPanelVisibility))]
        [NotifyPropertyChangedFor(nameof(SingleDateVisibility))]
        private bool isRecurrent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CustomDaysVisibility))]
        private string recurrenceType = string.Empty;

        public Visibility RecurrentPanelVisibility
        {
            get
            {
                if (this.IsRecurrent)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility SingleDateVisibility
        {
            get
            {
                if (this.IsRecurrent)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public Visibility CustomDaysVisibility
        {
            get
            {
                if (this.RecurrenceType == CustomRecurrenceType)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        partial void OnFlightNumberSearchQueryChanged(string value)
        {
            this.SearchFlightsByNumber(value);
        }

        public void InitializeCompanyDashboard(int companyId)
        {
            this.currentCompanyId = companyId;
            this.RefreshAirportsList();
            this.RefreshCompanyFlights(companyId);
        }

        public void RefreshRunwaysList()
        {
            List<Runway> allRunways = runwayService.GetAllRunways();
            RunwaysList = new ObservableCollection<Runway>(allRunways);
        }

        public void RefreshGatesList()
        {
            List<Gate> allGates = gateService.GetAllGates();
            GatesList = new ObservableCollection<Gate>(allGates);
        }

        public void RefreshAirportsList()
        {
            List<Airport> airports = airportService.GetAllAirports();
            AirportsList = new ObservableCollection<Airport>(airports);
        }

        public void RefreshCompanyFlights(int companyId)
        {
            this.masterFlightsCollection = flightRouteService.GetFlightsByCompanyId(companyId);
            this.CompanyFlightsList = new ObservableCollection<Flight>(this.masterFlightsCollection);
        }

        public void SearchFlightsByNumber(string searchQuery)
        {
            List<Flight> filteredResults = flightRouteService.SearchFlightsByNumber(this.masterFlightsCollection, searchQuery);
            this.UpdateVisibleFlights(filteredResults);
        }

        private void UpdateVisibleFlights(List<Flight> flightsToDisplay)
        {
            CompanyFlightsList = new ObservableCollection<Flight>(flightsToDisplay);
        }

        [RelayCommand]
        private void ExecuteFlightDeletion(int flightId)
        {
            try
            {
                employeeFlightService.RemoveAllCrewAssignmentsForFlight(flightId);
                flightRouteService.DeleteFlightUsingId(flightId);
                this.RefreshCompanyFlights(this.currentCompanyId);
            }
            catch (Exception exception)
            {
                //--intent: silent catch for bulk operations.
            }
        }

        [RelayCommand]
        public void AddFlightFromInputs()
        {
            int.TryParse(this.CapacityText, out int capacityValue);

            flightRouteService.CreateFlightWithSchedule(
                this.currentCompanyId,
                this.SelectedRouteType,
                this.SelectedAirport?.Id ?? DefaultIdInCaseOfNull,
                capacityValue,
                this.DepartureTime,
                this.ArrivalTime,
                this.IsRecurrent,
                this.StartDate?.DateTime,
                this.EndDate?.DateTime,
                this.SingleDate?.DateTime,
                this.RecurrenceType,
                this.CustomDaysText,
                this.SelectedRunway?.Id ?? DefaultIdInCaseOfNull,
                this.SelectedGate?.Id ?? DefaultIdInCaseOfNull,
                companyService.GenerateFlightCodeUsingCompanyId);

            this.RefreshCompanyFlights(this.currentCompanyId);
            this.ResetInputFields();
        }

        public void ResetInputFields()
        {
            this.SelectedRouteType = null;
            this.SelectedAirport = null;
            this.CapacityText = string.Empty;
            this.IsRecurrent = false;
            this.SelectedRunway = null;
            this.SelectedGate = null;

            this.SingleDate = DateTimeOffset.Now;
            this.StartDate = DateTimeOffset.Now;
            this.EndDate = DateTimeOffset.Now.AddDays(DefaultEndRecurrenceInterval);

            this.DepartureTime = new TimeSpan(DefaultDepartureHour, DefaultMinute, 0);
            this.ArrivalTime = new TimeSpan(DefaultArrivalHour, DefaultMinute, 0);

            this.CustomDaysText = string.Empty;
        }
    }
}