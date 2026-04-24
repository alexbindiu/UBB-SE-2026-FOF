using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.Data.Services.Interfaces;

namespace TicketSellingModule.ViewModel
{
    /// <summary>
    /// Represents the types of entities manageable within the airport dashboard.
    /// </summary>
    public enum AirportDashboardEntity
    {
        None,
        Runway,
        Gate,
        Airport
    }

    public partial class AirportDashboardViewModel(
        IAirportService airportService,
        IRunwayService runwayService,
        IGateService gateService) : ObservableObject
    {
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
        [ObservableProperty] private string deleteWarningMessage = string.Empty;

        private object? itemPendingDeletion;
        private AirportDashboardEntity currentActiveEntity = AirportDashboardEntity.None;

        [RelayCommand]
        public void LoadDashboardData()
        {
            var allRunways = runwayService.GetAllRunways();
            RunwaysList.Clear();
            foreach (Runway runway in allRunways)
            {
                RunwaysList.Add(runway);
            }

            var allGates = gateService.GetAllGates();
            GatesList.Clear();
            foreach (Gate gate in allGates)
            {
                GatesList.Add(gate);
            }

            var allAirports = airportService.GetAllAirports();
            AirportsList.Clear();
            foreach (Airport airport in allAirports)
            {
                AirportsList.Add(airport);
            }
        }

        [RelayCommand]
        private void PrepareNewRunwayDialog()
        {
            currentActiveEntity = AirportDashboardEntity.Runway;
            EditingId = 0;
            EditingName = string.Empty;
            EditingHandleTimeText = string.Empty;

            HandleTimeVisibility = Visibility.Visible;
            CityCodeVisibility = Visibility.Collapsed;

            DialogTitle = "Register New Runway";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void PrepareEditRunwayDialog()
        {
            if (SelectedRunway == null)
            {
                return;
            }

            currentActiveEntity = AirportDashboardEntity.Runway;
            EditingId = SelectedRunway.Id;
            EditingName = SelectedRunway.Name;
            EditingHandleTimeText = SelectedRunway.HandleTime.ToString();

            HandleTimeVisibility = Visibility.Visible;
            CityCodeVisibility = Visibility.Collapsed;

            DialogTitle = "Edit Existing Runway";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void PrepareNewGateDialog()
        {
            currentActiveEntity = AirportDashboardEntity.Gate;
            EditingId = 0;
            EditingName = string.Empty;

            HandleTimeVisibility = Visibility.Collapsed;
            CityCodeVisibility = Visibility.Collapsed;

            DialogTitle = "Register New Gate";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void PrepareEditGateDialog()
        {
            if (SelectedGate == null)
            {
                return;
            }

            currentActiveEntity = AirportDashboardEntity.Gate;
            EditingId = SelectedGate.Id;
            EditingName = SelectedGate.Name;

            HandleTimeVisibility = Visibility.Collapsed;
            CityCodeVisibility = Visibility.Collapsed;

            DialogTitle = "Edit Existing Gate";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void PrepareNewAirportDialog()
        {
            currentActiveEntity = AirportDashboardEntity.Airport;
            EditingId = 0;
            EditingName = string.Empty;
            EditingCity = string.Empty;
            EditingCode = string.Empty;

            HandleTimeVisibility = Visibility.Collapsed;
            CityCodeVisibility = Visibility.Visible;

            DialogTitle = "Register New Airport";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void PrepareEditAirportDialog()
        {
            if (SelectedAirport == null)
            {
                return;
            }

            currentActiveEntity = AirportDashboardEntity.Airport;
            EditingId = SelectedAirport.Id;
            EditingName = SelectedAirport.AirportName;
            EditingCity = SelectedAirport.City;
            EditingCode = SelectedAirport.AirportCode;

            HandleTimeVisibility = Visibility.Collapsed;
            CityCodeVisibility = Visibility.Visible;

            DialogTitle = "Edit Existing Airport";
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void CloseConfigurationDialog()
        {
            DialogVisibility = Visibility.Collapsed;
        }

        [RelayCommand]
        private void SaveDialogChanges()
        {
            try
            {
                DialogErrorMessage = string.Empty;
                this.ProcessEntitySave(
                    currentActiveEntity,
                    EditingId,
                    EditingName,
                    EditingHandleTimeText,
                    EditingCity,
                    EditingCode);

                this.LoadDashboardData();
                DialogVisibility = Visibility.Collapsed;
            }
            catch (Exception exception)
            {
                DialogErrorMessage = exception.Message;
            }
        }

        private void ProcessEntitySave(AirportDashboardEntity entityType, int id, string name, string handleTimeText, string city, string code)
        {
            if (entityType == AirportDashboardEntity.Runway)
            {
                runwayService.SaveRunway(id, name, handleTimeText);
                return;
            }

            if (entityType == AirportDashboardEntity.Gate)
            {
                gateService.SaveGate(id, name);
                return;
            }

            if (entityType == AirportDashboardEntity.Airport)
            {
                if (id == 0)
                {
                    airportService.AddAirport(code, name, city);
                }
                else
                {
                    airportService.UpdateAirport(id, city, name, code);
                }
                return;
            }

            throw new ArgumentException("The specified entity type is not supported for saving.");
        }

        [RelayCommand]
        private void CloseDeleteConfirmation()
        {
            DeleteConfirmationVisibility = Visibility.Collapsed;
            itemPendingDeletion = null;
        }

        [RelayCommand]
        private void ExecuteDeletion()
        {
            try
            {
                if (itemPendingDeletion != null)
                {
                    this.RemoveEntityFromSystem(itemPendingDeletion);
                    this.LoadDashboardData();
                }
            }
            catch (Exception exception)
            {
                DialogErrorMessage = $"The deletion operation failed: {exception.Message}";
            }
            finally
            {
                this.CloseDeleteConfirmation();
            }
        }

        private void RemoveEntityFromSystem(object item)
        {
            if (item is Runway runway)
            {
                runwayService.DeleteRunwayUsingId(runway.Id);
                return;
            }

            if (item is Gate gate)
            {
                gateService.DeleteGateUsingId(gate.Id);
                return;
            }

            if (item is Airport airport)
            {
                airportService.DeleteAirportUsingId(airport.Id);
                return;
            }

            throw new ArgumentException("The selected item is not a valid deletable entity.");
        }

        private string ConstructDeleteWarningMessage(object item)
        {
            if (item is Runway runway)
            {
                bool hasFlights = runwayService.HasFlights(runway.Id);
                if (hasFlights)
                {
                    return $"CRITICAL: Runway '{runway.Name}' has flights assigned. Deleting it will remove ALL associated flights. Proceed?";
                }
                return $"Are you sure you want to delete runway '{runway.Name}'?";
            }

            if (item is Gate gate)
            {
                bool hasFlights = gateService.HasFlights(gate.Id);
                if (hasFlights)
                {
                    return $"CRITICAL: Gate '{gate.Name}' has flights assigned. Deleting it will remove ALL associated flights. Proceed?";
                }
                return $"Are you sure you want to delete gate '{gate.Name}'?";
            }

            if (item is Airport airport)
            {
                bool hasFlights = airportService.HasFlights(airport.Id);
                if (hasFlights)
                {
                    return $"CRITICAL: Airport '{airport.AirportName}' has flights assigned. Deleting it will remove ALL associated flights. Proceed?";
                }
                return $"Are you sure you want to delete airport '{airport.AirportName}'?";
            }

            return "Are you sure you want to delete the selected item?";
        }

        [RelayCommand]
        private void PromptDeleteRunway()
        {
            if (SelectedRunway == null)
            {
                return;
            }

            itemPendingDeletion = SelectedRunway;
            DeleteWarningMessage = this.ConstructDeleteWarningMessage(itemPendingDeletion);
            DeleteConfirmationVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void PromptDeleteGate()
        {
            if (SelectedGate == null)
            {
                return;
            }

            itemPendingDeletion = SelectedGate;
            DeleteWarningMessage = this.ConstructDeleteWarningMessage(itemPendingDeletion);
            DeleteConfirmationVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void PromptDeleteAirport()
        {
            if (SelectedAirport == null)
            {
                return;
            }

            itemPendingDeletion = SelectedAirport;
            DeleteWarningMessage = this.ConstructDeleteWarningMessage(itemPendingDeletion);
            DeleteConfirmationVisibility = Visibility.Visible;
        }
    }
}