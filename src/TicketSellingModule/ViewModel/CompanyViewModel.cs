using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

namespace TicketSellingModule.ViewModel
{
    public partial class CompanyViewModel : ObservableObject
    {
        private readonly CompanyService companyService;
        private readonly AirportService airportService;
        private readonly RunwayService runwayService;
        private readonly GateService gateService;
        private readonly FlightRouteService flightRouteService;
        private readonly EmployeeFlightService employeeFlightService;

        private int currentCompanyId;
        private List<Flight> masterCompanyFlights = new();

        public ObservableCollection<Company> CompaniesList { get; } = new();
        public ObservableCollection<Airport> AirportsList { get; } = new();
        public ObservableCollection<Flight> CompanyFlightsList { get; } = new();

        public ObservableCollection<Runway> RunwaysList { get; } = new();
        public ObservableCollection<Gate> GatesList { get; } = new();

        public int CurrentCompanyId => currentCompanyId;

        [ObservableProperty] private string searchText;
        [ObservableProperty] private string selectedRouteType;
        [ObservableProperty] private Airport selectedAirport;
        [ObservableProperty] private string capacityText;
        [ObservableProperty] private TimeSpan departureTime = TimeSpan.Zero;
        [ObservableProperty] private TimeSpan arrivalTime = TimeSpan.Zero;
        [ObservableProperty] private DateTimeOffset? singleDate;
        [ObservableProperty] private string customDaysText;
        [ObservableProperty] private DateTimeOffset? startDate;
        [ObservableProperty] private DateTimeOffset? endDate;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RecurrentPanelVisibility))]
        [NotifyPropertyChangedFor(nameof(SingleDateVisibility))]
        private bool isRecurrent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CustomDaysVisibility))]
        private string recurrenceType;

        public Visibility RecurrentPanelVisibility => IsRecurrent ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SingleDateVisibility => IsRecurrent ? Visibility.Collapsed : Visibility.Visible;
        public Visibility CustomDaysVisibility => RecurrenceType == "Custom" ? Visibility.Visible : Visibility.Collapsed;

        public CompanyViewModel(CompanyService companyService,
            AirportService airportService,
            FlightRouteService flightRouteService, RunwayService runwayService, GateService gateService, EmployeeFlightService employeeFlightService)
        {
            this.companyService = companyService;
            this.airportService = airportService;
            this.flightRouteService = flightRouteService;
            this.runwayService = runwayService;
            this.gateService = gateService;
            this.employeeFlightService = employeeFlightService;
        }

        private Runway selectedRunway;
        public Runway SelectedRunway
        {
            get => selectedRunway;
            set
            {
                selectedRunway = value;
                OnPropertyChanged();
            }
        }

        private Gate selectedGate;
        public Gate SelectedGate
        {
            get => selectedGate;
            set
            {
                selectedGate = value;
                OnPropertyChanged();
            }
        }

        partial void OnSearchTextChanged(string value) => SearchFlights(value);

        public void InitializeCompany(int companyId)
        {
            currentCompanyId = companyId;
            GetAllAirports();
            GetCompanyFlights(companyId);
        }

        public void LoadRunways()
        {
            var runways = runwayService.GetAll();
            RunwaysList.Clear();
            foreach (var runway in runways)
            {
                RunwaysList.Add(runway);
            }
        }

        public void LoadGates()
        {
            var gates = gateService.GetAll();
            GatesList.Clear();
            foreach (var gate in gates)
            {
                GatesList.Add(gate);
            }
        }

        public List<Company> GetAllCompanies()
        {
            var companies = companyService.GetAll();
            CompaniesList.Clear();
            foreach (var company in companies)
            {
                CompaniesList.Add(company);
            }

            return companies;
        }

        public Company GetCompanyById(int companyId) =>
            companyService.GetCompanyById(companyId);

        public List<Airport> GetAllAirports()
        {
            var airports = airportService.GetAll();
            AirportsList.Clear();
            foreach (var airport in airports)
            {
                AirportsList.Add(airport);
            }

            return airports;
        }

        public void GetCompanyFlights(int companyId)
        {
            var allRoutes = flightRouteService.GetAllRoutes() ?? new List<Route>();
            var companyRouteIds = allRoutes.Where(r => r.Company.Id == companyId).Select(r => r.Id).ToList();

            var allFlightsWithDetails = flightRouteService.GetAllFlightsWithDetails() ?? new List<Flight>();
            masterCompanyFlights = allFlightsWithDetails.Where(f => companyRouteIds.Contains(f.Route.Id)).ToList();

            CompanyFlightsList.Clear();
            foreach (var flight in masterCompanyFlights)
            {
                CompanyFlightsList.Add(flight);
            }
        }

        public void SearchFlights(string searchText)
        {
            CompanyFlightsList.Clear();

            var source = string.IsNullOrWhiteSpace(searchText)
                ? masterCompanyFlights
                : masterCompanyFlights.Where(f =>
                    !string.IsNullOrEmpty(f.FlightNumber) &&
                    f.FlightNumber.ToLower().Contains(searchText.ToLower())).ToList();

            foreach (var flight in source)
            {
                CompanyFlightsList.Add(flight);
            }
        }

        [RelayCommand]
        private void DeleteFlight(int flightId)
        {
            if (currentCompanyId == 0)
            {
                return;
            }

            try
            {
                employeeFlightService.CleanUpFlightAssignments(flightId);
                flightRouteService.DeleteFlight(flightId);

                GetCompanyFlights(currentCompanyId);
            }
            catch (Exception ex)
            {
            }
        }

        public void AddFlightFromInputs()
        {
            if (currentCompanyId == 0)
            {
                throw new InvalidOperationException("Company not selected.");
            }

            if (SelectedAirport == null || SelectedRunway == null || SelectedGate == null)
            {
                throw new InvalidOperationException("Please fill all required fields.");
            }

            if (!int.TryParse(CapacityText, out int capacity))
            {
                throw new InvalidOperationException("Invalid capacity.");
            }

            string typeCode = SelectedRouteType == "Arrival" ? "ARR" : "DEP";

            flightRouteService.CreateFlightWithSchedule(
                currentCompanyId,
                typeCode,
                SelectedAirport.Id,
                capacity,
                DepartureTime,
                ArrivalTime,
                IsRecurrent,
                StartDate?.DateTime,
                EndDate?.DateTime,
                SingleDate?.DateTime,
                RecurrenceType,
                CustomDaysText,
                SelectedRunway.Id,
                SelectedGate.Id,
                companyService.GenerateFlightCode);

            GetCompanyFlights(currentCompanyId);
            ClearInputs();
        }

        public void ClearInputs()
        {
            SelectedRouteType = null;
            SelectedAirport = null;
            CapacityText = string.Empty;
            IsRecurrent = false;

            SelectedRunway = null;
            SelectedGate = null;

            SingleDate = DateTimeOffset.Now;
            StartDate = DateTimeOffset.Now;
            EndDate = DateTimeOffset.Now.AddDays(7);

            DepartureTime = new TimeSpan(12, 0, 0);
            ArrivalTime = new TimeSpan(13, 0, 0);

            CustomDaysText = string.Empty;
        }
    }
}
