using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using CommunityToolkit.Mvvm.Input;
using TicketSellingModule.Data.Domain;
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class CompanyViewModel : INotifyPropertyChanged
    {
        private readonly ICompanyService companyService;
        private readonly IAirportService airportService;
        private readonly IFlightRouteService flightRouteService;
        private readonly IEmployeeFlightService employeeFlightService;
        private readonly IRunwayService runwayService;
        private readonly IGateService gateService;

        private const string CustomRecurrenceType = "Custom";
        private const int DefaultEndRecurrenceInterval = 7;
        private const int DefaultDepartureHour = 12;
        private const int DefaultArrivalHour = 13;
        private const int DefaultMinute = 0;
        private const int DefaultIdInCaseOfNull = 0;

        private int currentCompanyId;
        private List<Flight> masterFlightsCollection = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public CompanyViewModel(
            ICompanyService companyService,
            IAirportService airportService,
            IFlightRouteService flightRouteService,
            IEmployeeFlightService employeeFlightService,
            IRunwayService runwayService,
            IGateService gateService)
        {
            this.companyService = companyService;
            this.airportService = airportService;
            this.flightRouteService = flightRouteService;
            this.employeeFlightService = employeeFlightService;
            this.runwayService = runwayService;
            this.gateService = gateService;

            ExecuteFlightDeletionCommand = new RelayCommand<int>(ExecuteFlightDeletion);
            AddFlightFromInputsCommand = new RelayCommand(AddFlightFromInputs);
        }

        private ObservableCollection<Airport> airportsList;
        public ObservableCollection<Airport> AirportsList
        {
            get => airportsList;
            set
            {
                airportsList = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Flight> companyFlightsList;
        public ObservableCollection<Flight> CompanyFlightsList
        {
            get => companyFlightsList;
            set
            {
                companyFlightsList = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Runway> runwaysList;
        public ObservableCollection<Runway> RunwaysList
        {
            get => runwaysList;
            set
            {
                runwaysList = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Gate> gatesList;
        public ObservableCollection<Gate> GatesList
        {
            get => gatesList;
            set
            {
                gatesList = value;
                OnPropertyChanged();
            }
        }

        private string flightNumberSearchQuery = string.Empty;
        public string FlightNumberSearchQuery
        {
            get => flightNumberSearchQuery;
            set
            {
                if (flightNumberSearchQuery != value)
                {
                    flightNumberSearchQuery = value;
                    OnPropertyChanged();
                    this.SearchFlightsByNumber(value);
                }
            }
        }

        private string? selectedRouteType;
        public string? SelectedRouteType
        {
            get => selectedRouteType;
            set
            {
                selectedRouteType = value;
                OnPropertyChanged();
            }
        }

        private Airport? selectedAirport;
        public Airport? SelectedAirport
        {
            get => selectedAirport;
            set
            {
                selectedAirport = value;
                OnPropertyChanged();
            }
        }

        private string capacityText = string.Empty;
        public string? CapacityText
        {
            get => capacityText;
            set
            {
                capacityText = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan departureTime = TimeSpan.Zero;
        public TimeSpan DepartureTime
        {
            get => departureTime;
            set
            {
                departureTime = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan arrivalTime = TimeSpan.Zero;
        public TimeSpan ArrivalTime
        {
            get => arrivalTime;
            set
            {
                arrivalTime = value;
                OnPropertyChanged();
            }
        }

        public DateTimeOffset? singleDate;
        public DateTimeOffset? SingleDate
        {
            get => singleDate;
            set
            {
                singleDate = value;
                OnPropertyChanged();
            }
        }

        private string customDaysText = string.Empty;
        public string CustomDaysText
        {
            get => customDaysText;
            set
            {
                customDaysText = value;
                OnPropertyChanged();
            }
        }

        private DateTimeOffset? startDate;
        public DateTimeOffset? StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                OnPropertyChanged();
            }
        }

        private DateTimeOffset? endDate;
        public DateTimeOffset? EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                OnPropertyChanged();
            }
        }

        private Runway? selectedRunway;
        public Runway? SelectedRunway
        {
            get => selectedRunway;
            set
            {
                selectedRunway = value;
                OnPropertyChanged();
            }
        }

        private Gate? selectedGate;
        public Gate? SelectedGate
        {
            get => selectedGate;
            set
            {
                selectedGate = value;
                OnPropertyChanged();
            }
        }

        private bool isRecurrent;
        public bool IsRecurrent
        {
            get => isRecurrent;
            set
            {
                if (isRecurrent != value)
                {
                    isRecurrent = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RecurrentPanelVisibility));
                    OnPropertyChanged(nameof(SingleDateVisibility));
                }
            }
        }

        private string? recurrenceType = string.Empty;
        public string RecurrenceType
        {
            get => recurrenceType;
            set
            {
                if (recurrenceType != value)
                {
                    recurrenceType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CustomDaysVisibility));
                }
            }
        }

        public Visibility RecurrentPanelVisibility => IsRecurrent ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SingleDateVisibility => IsRecurrent ? Visibility.Collapsed : Visibility.Visible;
        public Visibility CustomDaysVisibility => RecurrenceType == CustomRecurrenceType ? Visibility.Visible : Visibility.Collapsed;

        public IRelayCommand<int> ExecuteFlightDeletionCommand { get; }
        public IRelayCommand AddFlightFromInputsCommand { get; }

        public void InitializeCompanyDashboard(int companyId)
        {
            this.currentCompanyId = companyId;
            this.RefreshAirportsList();
            this.RefreshCompanyFlights(companyId);
            this.RefreshRunwaysList();
            this.RefreshGatesList();
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

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}