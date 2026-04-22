using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

namespace TicketSellingModule.ViewModel
{
    public partial class CompanyViewModel : ObservableObject
    {
        private readonly CompanyService _companyService;
        private readonly AirportService _airportService;
        private readonly RunwayService _runwayService;
        private readonly GateService _gateService;
        private readonly FlightRouteService _flightRouteService;
        private readonly EmployeeFlightService _employeeFlightService;

        private int _currentCompanyId;
        private List<Flight> _masterCompanyFlights = new();

        public ObservableCollection<Company> CompaniesList { get; } = new();
        public ObservableCollection<Airport> AirportsList { get; } = new();
        public ObservableCollection<Flight> CompanyFlightsList { get; } = new();

        public ObservableCollection<Runway> RunwaysList { get; } = new();
        public ObservableCollection<Gate> GatesList { get; } = new();

        public int CurrentCompanyId => _currentCompanyId;

        [ObservableProperty] private string _searchText;
        [ObservableProperty] private string _selectedRouteType;
        [ObservableProperty] private Airport _selectedAirport;
        [ObservableProperty] private string _capacityText;
        [ObservableProperty] private TimeSpan _departureTime = TimeSpan.Zero;
        [ObservableProperty] private TimeSpan _arrivalTime = TimeSpan.Zero;
        [ObservableProperty] private DateTimeOffset? _singleDate;
        [ObservableProperty] private string _customDaysText;
        [ObservableProperty] private DateTimeOffset? _startDate;
        [ObservableProperty] private DateTimeOffset? _endDate;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RecurrentPanelVisibility))]
        [NotifyPropertyChangedFor(nameof(SingleDateVisibility))]
        private bool _isRecurrent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CustomDaysVisibility))]
        private string _recurrenceType;

        

        public Visibility RecurrentPanelVisibility => IsRecurrent ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SingleDateVisibility => IsRecurrent ? Visibility.Collapsed : Visibility.Visible;
        public Visibility CustomDaysVisibility => RecurrenceType == "Custom" ? Visibility.Visible : Visibility.Collapsed;

        public CompanyViewModel(CompanyService companyService,
            AirportService airportService,
            FlightRouteService flightRouteService, RunwayService runwayService, GateService gateService, EmployeeFlightService employeeFlightService    )
        {
            _companyService = companyService;
            _airportService = airportService;
            _flightRouteService = flightRouteService;
            _runwayService = runwayService;
            _gateService = gateService;
            _employeeFlightService = employeeFlightService;
        }


        private Runway _selectedRunway;
        public Runway SelectedRunway
        {
            get => _selectedRunway;
            set { _selectedRunway = value; OnPropertyChanged(); }
        }

        private Gate _selectedGate;
        public Gate SelectedGate
        {
            get => _selectedGate;
            set { _selectedGate = value; OnPropertyChanged(); }
        }


        partial void OnSearchTextChanged(string value) => SearchFlights(value);

        public void InitializeCompany(int companyId)
        {
            _currentCompanyId = companyId;
            GetAllAirports();
            GetCompanyFlights(companyId);
        }

        public void LoadRunways()
        {
            var runways = _runwayService.GetAll();
            RunwaysList.Clear();
            foreach (var runway in runways)
            {
                RunwaysList.Add(runway);
            }
        }

        public void LoadGates()
        {
            var gates = _gateService.GetAll();
            GatesList.Clear();
            foreach (var gate in gates)
            {
                GatesList.Add(gate);
            }
        }

        public List<Company> GetAllCompanies()
        {
            var companies = _companyService.GetAll();
            CompaniesList.Clear();
            foreach (var company in companies) CompaniesList.Add(company);
            return companies;
        }

        public Company GetCompanyById(int companyId) =>
            _companyService.GetCompanyById(companyId);

        public List<Airport> GetAllAirports()
        {
            var airports = _airportService.GetAll();
            AirportsList.Clear();
            foreach (var airport in airports) AirportsList.Add(airport);
            return airports;
        }

        

        

        public void GetCompanyFlights(int companyId)
        {
            var allRoutes = _flightRouteService.GetAllRoutes() ?? new List<Route>();
            var companyRouteIds = allRoutes.Where(r => r.Company.Id == companyId).Select(r => r.Id).ToList();

            var allFlights = _flightRouteService.GetAllFlights() ?? new List<Flight>();
            _masterCompanyFlights = allFlights.Where(f => companyRouteIds.Contains(f.Route.Id)).ToList();

            CompanyFlightsList.Clear();
            foreach (var flight in _masterCompanyFlights) CompanyFlightsList.Add(flight);
        }

        public void SearchFlights(string searchText)
        {
            CompanyFlightsList.Clear();

            var source = string.IsNullOrWhiteSpace(searchText)
                ? _masterCompanyFlights
                : _masterCompanyFlights.Where(f =>
                    !string.IsNullOrEmpty(f.FlightNumber) &&
                    f.FlightNumber.ToLower().Contains(searchText.ToLower())).ToList();

            foreach (var flight in source) CompanyFlightsList.Add(flight);
        }

        [RelayCommand]
        private void DeleteFlight(int flightId)
        {
            if (_currentCompanyId == 0) return;
            try
            {
                
                _employeeFlightService.CleanUpFlightAssignments(flightId);
                _flightRouteService.DeleteFlight(flightId);

                GetCompanyFlights(_currentCompanyId);
            }
            catch (Exception ex)
            {
            }
        }

        public void AddFlightFromInputs()
        {
            
            if (_currentCompanyId == 0) throw new InvalidOperationException("Company not selected.");
            if (SelectedAirport == null || SelectedRunway == null || SelectedGate == null)
                throw new InvalidOperationException("Please fill all required fields.");

            if (!int.TryParse(CapacityText, out int capacity))
                throw new InvalidOperationException("Invalid capacity.");

            string typeCode = SelectedRouteType == "Arrival" ? "ARR" : "DEP";

            
            _flightRouteService.CreateFlightWithSchedule(
                _currentCompanyId,
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
                _companyService.GenerateFlightCode
            );

            GetCompanyFlights(_currentCompanyId);
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
