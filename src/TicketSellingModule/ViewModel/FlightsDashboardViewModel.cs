using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Data.Services;
using TicketSellingModule.Data.Services.Interfaces;
using TicketSellingModule.WinUI.AirportAdmin.Components;

namespace TicketSellingModule.ViewModel
{
    public partial class FlightsDashboardViewModel : INotifyPropertyChanged
    {
        private readonly IFlightRouteService flightRouteService;
        private readonly IEmployeeFlightService flightEmployeeService;

        private List<Flight> allFlights = new();

        private string searchText = string.Empty;
        private FlightDisplayRow? selectedFlight;
        private Visibility crewDialogVisibility = Visibility.Collapsed;
        private string dialogError = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public FlightsDashboardViewModel(
           IFlightRouteService flightRouteService,
           IEmployeeFlightService flightEmployeeService)
        {
            this.flightRouteService = flightRouteService;
            this.flightEmployeeService = flightEmployeeService;
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        public FlightDisplayRow? SelectedFlight
        {
            get => selectedFlight;
            set
            {
                if (selectedFlight != value)
                {
                    selectedFlight = value;
                    OnPropertyChanged();
                }
            }
        }

        public Visibility CrewDialogVisibility
        {
            get => crewDialogVisibility;
            set
            {
                if (crewDialogVisibility != value)
                {
                    crewDialogVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DialogError
        {
            get => dialogError;
            set
            {
                if (dialogError != value)
                {
                    dialogError = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<CrewSelectionWrapper> AvailableCrew { get; } = new();
        public ObservableCollection<FlightDisplayRow> FilteredFlights { get; } = new();

        [RelayCommand]
        public void LoadFlights()
        {
            allFlights = flightRouteService.GetAllFlightsWithDetails();
            ApplyFilter();
        }

        [RelayCommand]
        private void OpenCrewManagement()
        {
            if (SelectedFlight == null)
            {
                return;
            }

            Flight? flight = flightRouteService.GetFlightById(SelectedFlight.Id);

            List<CrewMemberSelectionData> crewData = flightEmployeeService.GetCrewSelectionData(flight);

            AvailableCrew.Clear();
            foreach (CrewMemberSelectionData item in crewData)
            {
                AvailableCrew.Add(new CrewSelectionWrapper
                {
                    Employee = item.Employee,
                    IsSelected = item.IsSelected,
                    RoleHeader = item.RoleHeader,
                    RoleHeaderVisibility = item.IsFirstInRoleGroup ? Visibility.Visible : Visibility.Collapsed
                });
            }

            DialogError = string.Empty;
            CrewDialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void SaveCrew()
        {
            if (SelectedFlight == null)
            {
                return;
            }

            List<int> selectedEmployeeIds = new List<int>();
            foreach (CrewSelectionWrapper selectionContext in this.AvailableCrew)
            {
                if (selectionContext.IsSelected)
                {
                    selectedEmployeeIds.Add(selectionContext.Employee.Id);
                }
            }

            flightEmployeeService.UpdateEmployeesForFlightUsingIds(SelectedFlight.Id, selectedEmployeeIds);
            CrewDialogVisibility = Visibility.Collapsed;
            LoadFlights();
        }

        [RelayCommand]
        private void CloseDialog()
        {
            CrewDialogVisibility = Visibility.Collapsed;
        }

        private void ApplyFilter()
        {
            string query = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;
            List<Flight> matchingFlights = flightRouteService.SearchFlights(allFlights, query);

            FilteredFlights.Clear();
            foreach (Flight flight in matchingFlights)
            {
                string crewText = flightEmployeeService.FormatCrewList(flight.Id);
                FilteredFlights.Add(new FlightDisplayRow(flightRouteService.BuildFlightSummary(flight, crewText)));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CrewSelectionWrapper
    {
        public Employee Employee { get; set; } = new();
        public bool IsSelected { get; set; }
        public bool ShowRoleHeader { get; set; }
        public string RoleHeader { get; set; } = string.Empty;
        public Visibility RoleHeaderVisibility { get; set; } = Visibility.Collapsed;
    }
}
