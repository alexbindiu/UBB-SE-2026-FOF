using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Data.Services.Interfaces;

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
        private const string ArrivalText = "Arrival";
        private const string ArrivalCode = "ARR";
        private const string DepartureCode = "DEP";
        private const string CustomRecurrenceType = "Custom";

        private int currentCompanyId;
        private List<Flight> masterFlightsCollection = new();

        public ObservableCollection<Company> CompaniesList { get; } = new();
        public ObservableCollection<Airport> AirportsList { get; } = new();
        public ObservableCollection<Flight> CompanyFlightsList { get; } = new();
        public ObservableCollection<Runway> RunwaysList { get; } = new();
        public ObservableCollection<Gate> GatesList { get; } = new();

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

        public int CurrentCompanyId
        {
            get
            {
                return this.currentCompanyId;
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
            this.RunwaysList.Clear();
            foreach (Runway runway in allRunways)
            {
                this.RunwaysList.Add(runway);
            }
        }

        public void RefreshGatesList()
        {
            List<Gate> allGates = gateService.GetAllGates();
            this.GatesList.Clear();
            foreach (Gate gate in allGates)
            {
                this.GatesList.Add(gate);
            }
        }

        public List<Company> GetAvailableCompanies()
        {
            List<Company> companies = companyService.GetAllCompanies();
            this.CompaniesList.Clear();
            foreach (Company company in companies)
            {
                this.CompaniesList.Add(company);
            }

            return companies;
        }

        public Company? GetCompanyById(int companyId)
        {
            return companyService.GetCompanyById(companyId);
        }

        public void RefreshAirportsList()
        {
            List<Airport> airports = airportService.GetAllAirports();
            this.AirportsList.Clear();
            foreach (Airport airport in airports)
            {
                this.AirportsList.Add(airport);
            }
        }

        public void RefreshCompanyFlights(int companyId)
        {
            List<Flight> companyFlights = flightRouteService.GetFlightsByCompanyId(companyId);
            this.masterFlightsCollection.Clear();

            foreach (Flight flight in companyFlights)
            {
                this.masterFlightsCollection.Add(flight);
            }

            this.UpdateVisibleFlights(this.masterFlightsCollection);
        }

        public void SearchFlightsByNumber(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                this.UpdateVisibleFlights(this.masterFlightsCollection);
                return;
            }

            List<Flight> filteredResults = new List<Flight>();
            string lowerSearchQuery = searchQuery.ToLower();

            foreach (Flight flight in this.masterFlightsCollection)
            {
                if (!string.IsNullOrEmpty(flight.FlightNumber))
                {
                    if (flight.FlightNumber.ToLower().Contains(lowerSearchQuery))
                    {
                        filteredResults.Add(flight);
                    }
                }
            }

            this.UpdateVisibleFlights(filteredResults);
        }

        private void UpdateVisibleFlights(List<Flight> flightsToDisplay)
        {
            this.CompanyFlightsList.Clear();
            foreach (Flight flight in flightsToDisplay)
            {
                this.CompanyFlightsList.Add(flight);
            }
        }

        [RelayCommand]
        private void ExecuteFlightDeletion(int flightId)
        {
            if (this.currentCompanyId == 0)
            {
                return;
            }

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
            int parsedCapacity = companyService.ValidateFlightCreationInputs(
                this.currentCompanyId,
                this.SelectedAirport?.Id ?? 0,
                this.CapacityText,
                this.SelectedRunway?.Id ?? 0,
                this.SelectedGate?.Id ?? 0);

            string flightTypeCode = this.SelectedRouteType == ArrivalText ? ArrivalCode : DepartureCode;

            flightRouteService.CreateFlightWithSchedule(
                this.currentCompanyId,
                flightTypeCode,
                this.SelectedAirport!.Id,
                parsedCapacity,
                this.DepartureTime,
                this.ArrivalTime,
                this.IsRecurrent,
                this.StartDate?.DateTime,
                this.EndDate?.DateTime,
                this.SingleDate?.DateTime,
                this.RecurrenceType,
                this.CustomDaysText,
                this.SelectedRunway.Id,
                this.SelectedGate.Id,
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
            this.EndDate = DateTimeOffset.Now.AddDays(7);

            this.DepartureTime = new TimeSpan(12, 0, 0);
            this.ArrivalTime = new TimeSpan(13, 0, 0);

            this.CustomDaysText = string.Empty;
        }
    }
}