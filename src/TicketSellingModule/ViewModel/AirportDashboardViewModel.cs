using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

namespace TicketSellingModule.ViewModel
{
    public partial class AirportDashboardViewModel : ObservableObject
    {
        private readonly AirportService airportService;
        private readonly RunwayService runwayService;
        private readonly GateService gateService;

        public ObservableCollection<Runway> RunwaysList { get; } = new();
        public ObservableCollection<Gate> GatesList { get; } = new();
        public ObservableCollection<Airport> AirportsList { get; } = new();

        [ObservableProperty] private Runway? selectedRunway;
        [ObservableProperty] private Gate? selectedGate;
        [ObservableProperty] private Airport? selectedAirport;

        [ObservableProperty] private Visibility dialogVisibility = Visibility.Collapsed;
        [ObservableProperty] private string dialogTitle = string.Empty;
        [ObservableProperty] private string dialogErrorMessage = string.Empty;
        [ObservableProperty] private Visibility handleTimeVisibility = Visibility.Collapsed;
        [ObservableProperty] private Visibility cityCodeVisibility = Visibility.Collapsed;

        [ObservableProperty] private int editingId;
        [ObservableProperty] private string editingName = string.Empty;
        [ObservableProperty] private string editingHandleTimeText = string.Empty;
        [ObservableProperty] private string editingCity = string.Empty;
        [ObservableProperty] private string editingCode = string.Empty;

        [ObservableProperty] private Visibility deleteConfirmationVisibility = Visibility.Collapsed;
        [ObservableProperty] private string deleteWarningMessage;
        private object itemToDelete;

        private string currentEntity = string.Empty;

        public AirportDashboardViewModel(AirportService airportService, RunwayService runwayService, GateService gateService)
        {
            this.airportService = airportService;
            this.runwayService = runwayService;
            this.gateService = gateService;
        }

        [RelayCommand]
        public void LoadData()
        {
            var runways = runwayService.GetAll();
            RunwaysList.Clear();
            foreach (var r in runways)
            {
                RunwaysList.Add(r);
            }

            var gates = gateService.GetAll();
            GatesList.Clear();
            foreach (var g in gates)
            {
                GatesList.Add(g);
            }

            var airports = airportService.GetAll();
            AirportsList.Clear();
            foreach (var a in airports)
            {
                AirportsList.Add(a);
            }
        }

        [RelayCommand]
        private void OpenAddRunway()
        {
            currentEntity = "Runway";
            EditingId = 0;
            EditingName = string.Empty;
            EditingHandleTimeText = string.Empty;

            HandleTimeVisibility = Visibility.Visible;
            CityCodeVisibility = Visibility.Collapsed;

            DialogTitle = "Add New Runway";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void OpenEditRunway()
        {
            if (SelectedRunway == null)
            {
                return;
            }

            currentEntity = "Runway";
            EditingId = SelectedRunway.Id;
            EditingName = SelectedRunway.Name;
            EditingHandleTimeText = SelectedRunway.HandleTime.ToString();

            HandleTimeVisibility = Visibility.Visible;
            CityCodeVisibility = Visibility.Collapsed;

            DialogTitle = "Edit Runway";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void OpenAddGate()
        {
            currentEntity = "Gate";
            EditingId = 0;
            EditingName = string.Empty;

            HandleTimeVisibility = Visibility.Collapsed;
            CityCodeVisibility = Visibility.Collapsed;

            DialogTitle = "Add New Gate";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void OpenEditGate()
        {
            if (SelectedGate == null)
            {
                return;
            }

            currentEntity = "Gate";
            EditingId = SelectedGate.Id;
            EditingName = SelectedGate.Name;

            HandleTimeVisibility = Visibility.Collapsed;
            CityCodeVisibility = Visibility.Collapsed;

            DialogTitle = "Edit Gate";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void OpenAddAirport()
        {
            currentEntity = "Airport";
            EditingId = 0;
            EditingName = string.Empty;
            EditingCity = string.Empty;
            EditingCode = string.Empty;

            HandleTimeVisibility = Visibility.Collapsed;
            CityCodeVisibility = Visibility.Visible;

            DialogTitle = "Add New Airport";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void OpenEditAirport()
        {
            if (SelectedAirport == null)
            {
                return;
            }

            currentEntity = "Airport";
            EditingId = SelectedAirport.Id;
            EditingName = SelectedAirport.AirportName;
            EditingCity = SelectedAirport.City;
            EditingCode = SelectedAirport.AirportCode;

            HandleTimeVisibility = Visibility.Collapsed;
            CityCodeVisibility = Visibility.Visible;

            DialogTitle = "Edit Airport";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void CloseDialog()
        {
            DialogVisibility = Visibility.Collapsed;
        }

        [RelayCommand]
        private void SaveDialog()
        {
            try
            {
                DialogErrorMessage = string.Empty;
                SaveDashboardEntity(
                    currentEntity,
                    EditingId,
                    EditingName,
                    EditingHandleTimeText,
                    EditingCity,
                    EditingCode);

                LoadData();
                DialogVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DialogErrorMessage = ex.Message;
            }
        }

        private void SaveDashboardEntity(string entityType, int editingId, string name, string handleTimeText, string city, string code)
        {
            if (entityType == "Runway")
            {
                runwayService.SaveRunway(editingId, name, handleTimeText);
                return;
            }

            if (entityType == "Gate")
            {
                gateService.SaveGate(editingId, name);
                return;
            }

            if (entityType == "Airport")
            {
                if (editingId == 0)
                {
                    airportService.Add(code, name, city);
                }
                else
                {
                    airportService.Update(editingId, city, name, code);
                }

                return;
            }

            throw new ArgumentException("Unsupported entity type.");
        }

        [RelayCommand]
        private void CloseDeleteDialog()
        {
            DeleteConfirmationVisibility = Visibility.Collapsed;
            itemToDelete = null;
        }

        [RelayCommand]
        private void ConfirmDelete()
        {
            try
            {
                DeleteDashboardEntity(itemToDelete);
                LoadData();
            }
            catch (Exception ex)
            {
                DialogErrorMessage = $"Delete failed: {ex.Message}";
            }
            finally
            {
                CloseDeleteDialog();
            }
        }

        private void DeleteDashboardEntity(object itemToDelete)
        {
            if (itemToDelete is Runway runway)
            {
                runwayService.Delete(runway.Id);
                return;
            }

            if (itemToDelete is Gate gate)
            {
                gateService.Delete(gate.Id);
                return;
            }

            if (itemToDelete is Airport airport)
            {
                airportService.Delete(airport.Id);
                return;
            }

            throw new ArgumentException("Invalid item selected for delete.");
        }

        private string BuildDeleteWarning(object itemToDelete)
        {
            if (itemToDelete is Runway runway)
            {
                bool hasFlights = runwayService.HasFlights(runway.Id);
                return hasFlights
                    ? $"Warning: Runway '{runway.Name}' has flights assigned. Deleting it will remove ALL associated flights. Continue?"
                    : $"Are you sure you want to delete runway '{runway.Name}'?";
            }

            if (itemToDelete is Gate gate)
            {
                bool hasFlights = gateService.HasFlights(gate.Id);
                return hasFlights
                    ? $"Warning: Gate '{gate.Name}' has flights assigned. Deleting it will remove ALL associated flights. Continue?"
                    : $"Are you sure you want to delete gate '{gate.Name}'?";
            }

            if (itemToDelete is Airport airport)
            {
                bool hasFlights = airportService.HasFlights(airport.Id);
                return hasFlights
                    ? $"Warning: Airport '{airport.AirportName}' has flights assigned. Deleting it will remove ALL associated flights. Continue?"
                    : $"Are you sure you want to delete airport '{airport.AirportName}'?";
            }

            throw new ArgumentException("Invalid item selected for delete.");
        }

        [RelayCommand]
        private void DeleteRunway()
        {
            if (SelectedRunway == null)
            {
                return;
            }

            itemToDelete = SelectedRunway;
            DeleteWarningMessage = BuildDeleteWarning(itemToDelete);
            DeleteConfirmationVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void DeleteGate()
        {
            if (SelectedGate == null)
            {
                return;
            }
            itemToDelete = SelectedGate;
            DeleteWarningMessage = BuildDeleteWarning(itemToDelete);
            DeleteConfirmationVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void DeleteAirport()
        {
            if (SelectedAirport == null)
            {
                return;
            }

            itemToDelete = SelectedAirport;
            DeleteWarningMessage = BuildDeleteWarning(itemToDelete);
            DeleteConfirmationVisibility = Visibility.Visible;
        }
    }
}
