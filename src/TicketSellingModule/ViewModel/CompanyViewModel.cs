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

        public int AddAirport(string airportCode, string city, string name)
        {
            int newId = airportService.Add(airportCode, name, city);
            GetAllAirports();
            return newId;
        }

        public int AddFlight(string flightNumber, int companyId, string routeType, int airportId,
                             int capacity, TimeOnly departureTime, TimeOnly arrivalTime,
                             int recurrenceInterval, DateTime startDate, DateTime endDate,
                             int runwayId, int gateId)
        {
            int newRouteId = flightRouteService.Add(
                companyId, airportId, routeType, recurrenceInterval,
                startDate, endDate, departureTime, arrivalTime,
                capacity, flightNumber, runwayId, gateId);

            GetCompanyFlights(companyId);
            return newRouteId;
        }

        public void GetCompanyFlights(int companyId)
        {
            var allRoutes = flightRouteService.GetAllRoutes() ?? new List<Route>();
            var companyRouteIds = allRoutes.Where(r => r.Company.Id == companyId).Select(r => r.Id).ToList();

            var allFlights = flightRouteService.GetAllFlights() ?? new List<Flight>();
            masterCompanyFlights = allFlights.Where(f => companyRouteIds.Contains(f.Route.Id)).ToList();

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
            DateTime? start = null;
            DateTime? end = null;

            if (currentCompanyId == 0)
            {
                throw new InvalidOperationException("Company not selected.");
            }

            if (string.IsNullOrWhiteSpace(SelectedRouteType))
            {
                throw new InvalidOperationException("Route type is required.");
            }

            if (SelectedAirport == null)
            {
                throw new InvalidOperationException("Airport is required.");
            }

            if (!int.TryParse(CapacityText, out int capacity) || capacity <= 0)
            {
                throw new InvalidOperationException("Capacity must be a positive number.");
            }

            if (SelectedRunway == null)
            {
                throw new InvalidOperationException("Please select a Runway.");
            }
            if (SelectedGate == null)
            {
                throw new InvalidOperationException("Please select a Gate.");
            }

            string type = SelectedRouteType switch
            {
                "Arrival" => "ARR",
                "Departure" => "DEP",
                _ => throw new InvalidOperationException("Invalid route type selected.")
            };

            bool isRecurrent = IsRecurrent;

            DateTime baseDate = isRecurrent ? StartDate.Value.DateTime.Date : SingleDate.Value.DateTime.Date;
            DateTime fullDep = baseDate.Add(DepartureTime);
            DateTime fullArr = baseDate.Add(ArrivalTime);

            if (fullArr <= fullDep)
            {
                fullArr = fullArr.AddDays(1);
            }
            TimeOnly depTime = TimeOnly.FromTimeSpan(DepartureTime);
            TimeOnly arrTime = TimeOnly.FromTimeSpan(ArrivalTime);
            if (fullArr == fullDep)
            {
                throw new InvalidOperationException("Arrival time cannot be the same as departure time.");
            }

            int interval = 0;

            if (IsRecurrent)
            {
                if (StartDate == null || EndDate == null)
                {
                    throw new InvalidOperationException("Start and end dates are required for recurrent flights.");
                }

                start = StartDate.Value.DateTime;
                end = EndDate.Value.DateTime;

                if (end < start)
                {
                    throw new InvalidOperationException("End date must be after start date.");
                }

                interval = RecurrenceType switch
                {
                    "Daily" => 1,
                    "Weekly" => 7,
                    "Monthly" => 30,
                    "Custom" => int.TryParse(CustomDaysText, out int custom) && custom > 0
                        ? custom
                        : throw new InvalidOperationException("Custom recurrence must be a positive number of days."),
                    _ => throw new InvalidOperationException("Recurrence type is required.")
                };
            }
            else
            {
                if (SingleDate == null)
                {
                    throw new InvalidOperationException("Flight date is required.");
                }

                start = SingleDate.Value.DateTime;
                end = start;
            }

            string flightNum = companyService.GenerateFlightCode(currentCompanyId);

            AddFlight(flightNum, currentCompanyId, type, SelectedAirport.Id,
                capacity, depTime, arrTime, interval, start.Value, end.Value, SelectedRunway.Id, SelectedGate.Id);

            GetCompanyFlights(currentCompanyId);
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
