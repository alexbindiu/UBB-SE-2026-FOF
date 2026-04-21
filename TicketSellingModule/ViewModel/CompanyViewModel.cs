using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Domain;
using TicketSellingModule.Service;

namespace TicketSellingModule.ViewModel
{
    public partial class CompanyViewModel : ObservableObject
    {
        private readonly CompanyService _companyService;
        private readonly AirportService _airportService;
        private readonly FlightRouteService _flightRouteService;

        private int _currentCompanyId;
        private List<Flight> _masterCompanyFlights = new();

        public ObservableCollection<Company> CompaniesList { get; } = new();
        public ObservableCollection<Airport> AirportsList { get; } = new();
        public ObservableCollection<Flight> CompanyFlightsList { get; } = new();

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

        public CompanyViewModel(
            CompanyService companyService,
            AirportService airportService,
            FlightRouteService flightRouteService)
        {
            _companyService = companyService;
            _airportService = airportService;
            _flightRouteService = flightRouteService;
        }

        // Called automatically by CommunityToolkit when SearchText changes
        partial void OnSearchTextChanged(string value) => SearchFlights(value);

        public void InitializeCompany(int companyId)
        {
            _currentCompanyId = companyId;
            GetAllAirports();
            GetCompanyFlights(companyId);
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
                capacity, flightNumber, runwayId, gateId);

            GetCompanyFlights(companyId);
            return newRouteId;
        }

        public void GetCompanyFlights(int companyId)
        {
            var allRoutes = _flightRouteService.GetAllRoutes() ?? new List<Route>();
            var companyRouteIds = allRoutes.Where(r => r.CompanyId == companyId).Select(r => r.Id).ToList();

            var allFlights = _flightRouteService.GetAllFlights() ?? new List<Flight>();
            _masterCompanyFlights = allFlights.Where(f => companyRouteIds.Contains(f.RouteId)).ToList();

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
            _flightRouteService.DeleteFlight(flightId);
            GetCompanyFlights(_currentCompanyId);
        }

        public void AddFlightFromInputs()
        {
            if (_currentCompanyId == 0)
                throw new InvalidOperationException("Company not selected.");
            if (string.IsNullOrWhiteSpace(SelectedRouteType))
                throw new InvalidOperationException("Route type is required.");
            if (SelectedAirport == null)
                throw new InvalidOperationException("Airport is required.");
            if (!int.TryParse(CapacityText, out int capacity) || capacity <= 0)
                throw new InvalidOperationException("Capacity must be a positive number.");

            string type = SelectedRouteType switch
            {
                "Arrival" => "ARR",
                "Departure" => "DEP",
                _ => throw new InvalidOperationException("Invalid route type selected.")
            };

            TimeOnly depTime = TimeOnly.FromTimeSpan(DepartureTime);
            TimeOnly arrTime = TimeOnly.FromTimeSpan(ArrivalTime);

            if (arrTime <= depTime)
                throw new InvalidOperationException("Arrival time must be after departure time.");

            DateTime start, end;
            int interval = 0;

            if (IsRecurrent)
            {
                if (StartDate == null || EndDate == null)
                    throw new InvalidOperationException("Start and end dates are required for recurrent flights.");

                start = StartDate.Value.DateTime;
                end = EndDate.Value.DateTime;

                if (end < start)
                    throw new InvalidOperationException("End date must be after start date.");

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
                    throw new InvalidOperationException("Flight date is required.");
                start = SingleDate.Value.DateTime;
                end = start;
            }

            // Business logic moved to CompanyService.GenerateFlightCode(int companyId)
            string flightNum = _companyService.GenerateFlightCode(_currentCompanyId);

            AddFlight(flightNum, _currentCompanyId, type, SelectedAirport.Id,
                capacity, depTime, arrTime, interval, start, end, 1, 1);

            GetCompanyFlights(_currentCompanyId);
        }
    }
}
