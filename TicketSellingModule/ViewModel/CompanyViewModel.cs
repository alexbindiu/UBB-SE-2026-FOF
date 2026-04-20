using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;
using TicketSellingModule.Service;

namespace TicketSellingModule.ViewModel
{
    public class CompanyViewModel : ViewModelBase
    {
        private readonly CompanyService _companyService;
        private readonly AirportService _airportService;
        private readonly FlightRouteService _flightRouteService;
        private int _currentCompanyId;
        private string _searchText;
        private string _selectedRouteType;
        private Airport _selectedAirport;
        private string _capacityText;
        private TimeSpan _departureTime = TimeSpan.Zero;
        private TimeSpan _arrivalTime = TimeSpan.Zero;
        private bool _isRecurrent;
        private DateTimeOffset? _singleDate;
        private string _recurrenceType;
        private string _customDaysText;
        private DateTimeOffset? _startDate;
        private DateTimeOffset? _endDate;
        
        public ObservableCollection<Company> CompaniesList { get; set; }
        public ObservableCollection<Airport> AirportsList { get; set; }
        public ObservableCollection<Flight> CompanyFlightsList { get; set; }
        private List<Flight> _masterCompanyFlights = new List<Flight>();

        public ICommand DeleteFlightCommand { get; }

        public CompanyViewModel()
        {
            var connectionFactory = new DbConnectionFactory();

            _companyService = new CompanyService(new CompanyRepo(connectionFactory));
            _airportService = new AirportService(new AirportRepo(connectionFactory));
            _flightRouteService = new FlightRouteService(new FlightRepo(connectionFactory), new RouteRepo(connectionFactory), new CompanyRepo(connectionFactory), new AirportRepo(connectionFactory));

            CompaniesList = new ObservableCollection<Company>();
            AirportsList = new ObservableCollection<Airport>();
            CompanyFlightsList = new ObservableCollection<Flight>();

            DeleteFlightCommand = new RelayCommand<int>(DeleteFlight);
        }

        public int CurrentCompanyId
        {
            get => _currentCompanyId;
            private set => SetProperty(ref _currentCompanyId, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SearchFlights(_searchText);
                }
            }
        }

        public string SelectedRouteType
        {
            get => _selectedRouteType;
            set => SetProperty(ref _selectedRouteType, value);
        }

        public Airport SelectedAirport
        {
            get => _selectedAirport;
            set => SetProperty(ref _selectedAirport, value);
        }

        public string CapacityText
        {
            get => _capacityText;
            set => SetProperty(ref _capacityText, value);
        }

        public TimeSpan DepartureTime
        {
            get => _departureTime;
            set => SetProperty(ref _departureTime, value);
        }

        public TimeSpan ArrivalTime
        {
            get => _arrivalTime;
            set => SetProperty(ref _arrivalTime, value);
        }

        public bool IsRecurrent
        {
            get => _isRecurrent;
            set
            {
                if (SetProperty(ref _isRecurrent, value))
                {
                    OnPropertyChanged(nameof(RecurrentPanelVisibility));
                    OnPropertyChanged(nameof(SingleDateVisibility));
                }
            }
        }

        public DateTimeOffset? SingleDate
        {
            get => _singleDate;
            set => SetProperty(ref _singleDate, value);
        }

        public string RecurrenceType
        {
            get => _recurrenceType;
            set
            {
                if (SetProperty(ref _recurrenceType, value))
                {
                    OnPropertyChanged(nameof(CustomDaysVisibility));
                }
            }
        }

        public string CustomDaysText
        {
            get => _customDaysText;
            set => SetProperty(ref _customDaysText, value);
        }

        public DateTimeOffset? StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTimeOffset? EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public Visibility RecurrentPanelVisibility => IsRecurrent ? Visibility.Visible : Visibility.Collapsed;

        public Visibility SingleDateVisibility => IsRecurrent ? Visibility.Collapsed : Visibility.Visible;

        public Visibility CustomDaysVisibility => RecurrenceType == "Custom" ? Visibility.Visible : Visibility.Collapsed;

        public List<Company> GetAllCompanies()
        {
            var companies = _companyService.GetAll();
            
            CompaniesList.Clear();
            foreach (var company in companies)
            {
                CompaniesList.Add(company);
            }
            
            return companies;
        }

        public Company GetCompanyById(int companyId)
        {
            return _companyService.GetCompanyById(companyId);
        }
        
        public List<Airport> GetAllAirports()
        {
            var airports = _airportService.GetAll();
            
            AirportsList.Clear();
            foreach (var airport in airports)
            {
                AirportsList.Add(airport);
            }
            
            return airports;
        }

        public void InitializeCompany(int companyId)
        {
            CurrentCompanyId = companyId;
            GetAllAirports();
            GetCompanyFlights(CurrentCompanyId);
        }

        public int AddAirport(string airportCode, string city, string name)
        {
            int newId = _airportService.Add(airportCode, name, city);
            
            GetAllAirports(); 
            return newId;
        }
        
        public int AddFlight(string flightNumber, int companyId, string routeType, int airportId, 
                             int capacity, TimeOnly departureTime, TimeOnly arrivalTime, 
                             int recurrenceInterval, DateTime startDate, DateTime endDate, 
                             int runwayId, int gateId)
        {
            int newRouteId = _flightRouteService.Add(
                companyId, airportId, routeType, recurrenceInterval, 
                startDate, endDate, departureTime, arrivalTime, 
                capacity, flightNumber, runwayId, gateId
            );
            
            GetCompanyFlights(companyId);

            return newRouteId;
        }

        public void GetCompanyFlights(int companyId)
        {
            var allRoutes = _flightRouteService.GetAllRoutes() ?? new List<Route>();
            var companyRouteIds = allRoutes.Where(r => r.CompanyId == companyId).Select(r => r.Id).ToList();

            var allFlights = _flightRouteService.GetAllFlights() ?? new List<Flight>();
            var companyFlights = allFlights.Where(f => companyRouteIds.Contains(f.RouteId)).ToList();

            _masterCompanyFlights = companyFlights;

            CompanyFlightsList.Clear();
            foreach (var flight in companyFlights)
            {
                CompanyFlightsList.Add(flight);
            }
        }
        public void SearchFlights(string searchText)
        {
            CompanyFlightsList.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var flight in _masterCompanyFlights)
                {
                    CompanyFlightsList.Add(flight);
                }
                return;
            }

            string lowerSearch = searchText.ToLower();

            var filteredList = _masterCompanyFlights.Where(f =>
                !string.IsNullOrEmpty(f.FlightNumber) &&
                f.FlightNumber.ToLower().Contains(lowerSearch)
            ).ToList();

            foreach (var flight in filteredList)
            {
                CompanyFlightsList.Add(flight);
            }
        }

        public void DeleteFlight(int flightId, int currentCompanyId)
        {
            _flightRouteService.DeleteFlight(flightId);
            
            GetCompanyFlights(currentCompanyId); 
        }

        public void DeleteFlight(int flightId)
        {
            if (CurrentCompanyId == 0)
            {
                return;
            }

            DeleteFlight(flightId, CurrentCompanyId);
        }

        public void AddFlightFromInputs()
        {
            if (CurrentCompanyId == 0)
            {
                throw new InvalidOperationException("Company not selected.");
            }

            int companyId = CurrentCompanyId;
            string flightNum = GenerateFlightCode(companyId);
            string type = SelectedRouteType == "Arrival" ? "ARR" : "DEP";
            int airportId = SelectedAirport?.Id ?? 0;

            int capacity = int.Parse(CapacityText ?? "0");
            TimeOnly depTime = TimeOnly.FromTimeSpan(DepartureTime);
            TimeOnly arrTime = TimeOnly.FromTimeSpan(ArrivalTime);

            bool isRecurrent = IsRecurrent;
            int interval = 0;
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;

            if (isRecurrent)
            {
                start = StartDate?.DateTime ?? DateTime.Now;
                end = EndDate?.DateTime ?? DateTime.Now.AddMonths(1);

                interval = RecurrenceType switch
                {
                    "Daily" => 1,
                    "Weekly" => 7,
                    "Monthly" => 30,
                    "Custom" => int.Parse(CustomDaysText ?? "0"),
                    _ => 0
                };
            }
            else
            {
                start = SingleDate?.DateTime ?? DateTime.Now;
                end = start;
            }

            AddFlight(flightNum, companyId, type, airportId, capacity,
                depTime, arrTime, interval, start, end, 1, 1);

            GetCompanyFlights(companyId);
        }
        public string GenerateFlightCode(int companyId)
        {
            var company = _companyService.GetCompanyById(companyId);
            string prefix = "FL";

            if (company != null && !string.IsNullOrEmpty(company.Name))
            {
                string[] words = company.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (words.Length >= 2)
                {
                    prefix = (words[0][0].ToString() + words[1][0].ToString()).ToUpper();
                }
                else if (company.Name.Length >= 2)
                {
                    prefix = company.Name.Substring(0, 2).ToUpper();
                }
            }

            Random rng = new Random();
            int flightNum = rng.Next(1000, 9999);

            return $"{prefix}-{flightNum}";
        }
    }
}