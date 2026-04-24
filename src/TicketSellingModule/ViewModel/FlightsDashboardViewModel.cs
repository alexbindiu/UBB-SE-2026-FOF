using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Data.Services.Interfaces;
using TicketSellingModule.WinUI.AirportAdmin.Components;

namespace TicketSellingModule.ViewModel
{
    public partial class FlightsDashboardViewModel(
       IFlightRouteService flightRouteService,
       IEmployeeFlightService flightEmployeeService) : ObservableObject
    {
        private List<Flight> allFlights = new();

        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private FlightRow? selectedFlight;

        [ObservableProperty] private Visibility crewDialogVisibility = Visibility.Collapsed;
        [ObservableProperty] private string dialogError = string.Empty;

        public ObservableCollection<CrewSelectionWrapper> AvailableCrew { get; } = new();
        public ObservableCollection<FlightRow> FilteredFlights { get; } = new();

        [RelayCommand]
        public void LoadFlights()
        {
            allFlights = flightRouteService.GetAllFlightsWithDetails();
            ApplyFilter();
        }

        partial void OnSearchTextChanged(string value)
        {
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
            if (flight == null)
            {
                return;
            }

            List<Employee> currentCrewMembers = flightEmployeeService.GetEmployeesAssignedToFlight(flight.Id);
            List<int> currentCrewIdentifiers = new List<int>();

            foreach (Employee crewMember in currentCrewMembers)
            {
                currentCrewIdentifiers.Add(crewMember.Id);
            }

            List<Employee> availableEmployees = flightEmployeeService.GetAvailableEmployeesGroupedByRole(flight);

            AvailableCrew.Clear();

            EmployeeRole? previousRole = null;

            foreach (Employee candidateEmployee in availableEmployees)
            {
                EmployeeRole currentRole = candidateEmployee.Role;

                bool isFirstInGroup = currentRole != previousRole;

                AvailableCrew.Add(new CrewSelectionWrapper
                {
                    Employee = candidateEmployee,
                    IsSelected = currentCrewIdentifiers.Contains(candidateEmployee.Id),
                    RoleHeader = currentRole.ToString(),
                    RoleHeaderVisibility = isFirstInGroup ? Visibility.Visible : Visibility.Collapsed
                });

                previousRole = currentRole;
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
            string query = this.SearchText?.Trim().ToLowerInvariant() ?? string.Empty;
            List<Flight> matchingFlights = new List<Flight>();

            foreach (Flight flight in this.allFlights)
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    matchingFlights.Add(flight);
                    continue;
                }

                if (this.IsFlightMatch(flight, query))
                {
                    matchingFlights.Add(flight);
                }
            }

            this.FilteredFlights.Clear();
            foreach (Flight flight in matchingFlights)
            {
                List<Employee> crew = flightEmployeeService.GetEmployeesAssignedToFlight(flight.Id);

                this.FilteredFlights.Add(new FlightRow
                {
                    Id = flight.Id,
                    FlightNumber = flight.FlightNumber ?? string.Empty,
                    DateText = flight.Date.ToString("dd.MM.yyyy HH:mm"),
                    DestinationText = flightRouteService.GetDestinationText(flight),
                    RunwayText = flight.Runway?.Name ?? "-",
                    GateText = flight.Gate?.Name ?? "-",
                    CrewText = this.FormatCrewList(crew)
                });
            }
        }

        private bool IsFlightMatch(Flight flight, string query)
        {
            if (flight.FlightNumber != null && flight.FlightNumber.ToLowerInvariant().Contains(query))
            {
                return true;
            }

            if (flight.Date.ToString("dd.MM.yyyy HH:mm").ToLowerInvariant().Contains(query))
            {
                return true;
            }

            string destination = flightRouteService.GetDestinationText(flight).ToLowerInvariant();
            if (destination.Contains(query))
            {
                return true;
            }

            if (flight.Runway?.Name != null && flight.Runway.Name.ToLowerInvariant().Contains(query))
            {
                return true;
            }

            if (flight.Gate?.Name != null && flight.Gate.Name.ToLowerInvariant().Contains(query))
            {
                return true;
            }

            return false;
        }

        private string FormatCrewList(List<Employee> crew)
        {
            if (crew.Count == 0)
            {
                return "Unassigned";
            }

            StringBuilder crewNames = new StringBuilder();
            for (int index = 0; index < crew.Count; index++)
            {
                crewNames.Append(crew[index].Name);
                if (index < crew.Count - 1)
                {
                    crewNames.Append(", ");
                }
            }

            return crewNames.ToString();
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