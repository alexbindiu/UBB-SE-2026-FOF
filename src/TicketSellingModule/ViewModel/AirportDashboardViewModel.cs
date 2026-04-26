using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

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
      IAirportService airportService, IRunwayService runwayService,
      IGateService gateService) : ObservableObject
    {
        private const string WarningConfirmationMessage = "Are you sure you want to delete the selected item?";
        private const string NewRunwayDialogTitle = "Register New Runway";
        private const string EditRunwayDialogTitle = "Edit Existing Runway";
        private const string NewGateDialogTitle = "Register New Gate";
        private const string EditGateDialogTitle = "Edit Existing Gate";
        private const string NewAirportDialogTitle = "Register New Airport";
        private const string EditAirportDialogTitle = "Edit Existing Airport";
        private const int DefaultEditingId = 0;

        [ObservableProperty] private ObservableCollection<Runway> runwaysList;
        [ObservableProperty] private ObservableCollection<Gate> gatesList;
        [ObservableProperty] private ObservableCollection<Airport> airportsList;
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

        private Dictionary<AirportDashboardEntity, Action> SaveRegistry => new()
        {
            { AirportDashboardEntity.Runway, () => runwayService.SaveRunway(EditingId, EditingName, EditingHandleTimeText) },
            { AirportDashboardEntity.Gate, () => gateService.SaveGate(EditingId, EditingName) },
            { AirportDashboardEntity.Airport, () => airportService.SaveAirport(EditingId, EditingCity, EditingName, EditingCode) }
        };

        private Dictionary<AirportDashboardEntity, Action<object>> DeleteRegistry => new()
        {
            { AirportDashboardEntity.Runway, itemToBeRemoved => runwayService.DeleteRunwayUsingId(((Runway)itemToBeRemoved).Id) },
            { AirportDashboardEntity.Gate, itemToBeRemoved => gateService.DeleteGateUsingId(((Gate)itemToBeRemoved).Id) },
            { AirportDashboardEntity.Airport, itemToBeRemoved => airportService.DeleteAirportUsingId(((Airport)itemToBeRemoved).Id) }
        };

        private Dictionary<AirportDashboardEntity, Func<object, string>> WarningRegistry => new()
        {
            { AirportDashboardEntity.Runway,  itemToBeRemoved => runwayService.GetDeleteWarningMessage(((Runway)itemToBeRemoved).Id) },
            { AirportDashboardEntity.Gate,    itemToBeRemoved => gateService.GetDeleteWarningMessage(((Gate)itemToBeRemoved).Id) },
            { AirportDashboardEntity.Airport, itemToBeRemoved => airportService.GetDeleteWarningMessage(((Airport)itemToBeRemoved).Id) }
        };

        private string ConstructDeleteWarningMessage(object item) =>
            WarningRegistry.TryGetValue(currentActiveEntity, out var getWarningMessage)
                ? getWarningMessage(item)
                : WarningConfirmationMessage;

        private object? itemPendingDeletion;
        private AirportDashboardEntity currentActiveEntity = AirportDashboardEntity.None;

        [RelayCommand]
        public void LoadDashboardData()
        {
            var allRunways = runwayService.GetAllRunways();
            RunwaysList = new ObservableCollection<Runway>(allRunways);

            var allGates = gateService.GetAllGates();
            GatesList = new ObservableCollection<Gate>(allGates);

            var allAirports = airportService.GetAllAirports();
            AirportsList = new ObservableCollection<Airport>(allAirports);
        }

        [RelayCommand]
        private void PrepareNewRunwayDialog()
        {
            currentActiveEntity = AirportDashboardEntity.Runway;
            EditingId = DefaultEditingId;
            EditingName = string.Empty;
            EditingHandleTimeText = string.Empty;

            HandleTimeVisibility = Visibility.Visible;
            CityCodeVisibility = Visibility.Collapsed;

            DialogTitle = NewRunwayDialogTitle;
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

            DialogTitle = EditRunwayDialogTitle;
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void PrepareNewGateDialog()
        {
            currentActiveEntity = AirportDashboardEntity.Gate;
            EditingId = DefaultEditingId;
            EditingName = string.Empty;

            HandleTimeVisibility = Visibility.Collapsed;
            CityCodeVisibility = Visibility.Collapsed;

            DialogTitle = NewGateDialogTitle;
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

            DialogTitle = EditGateDialogTitle;
            DialogErrorMessage = string.Empty;
            DialogVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void PrepareNewAirportDialog()
        {
            currentActiveEntity = AirportDashboardEntity.Airport;
            EditingId = DefaultEditingId;
            EditingName = string.Empty;
            EditingCity = string.Empty;
            EditingCode = string.Empty;

            HandleTimeVisibility = Visibility.Collapsed;
            CityCodeVisibility = Visibility.Visible;

            DialogTitle = NewAirportDialogTitle;
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

            DialogTitle = EditAirportDialogTitle;
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

                if (SaveRegistry.TryGetValue(currentActiveEntity, out var saveAction))
                {
                    saveAction();
                }

                this.LoadDashboardData();
                DialogVisibility = Visibility.Collapsed;
            }
            catch (Exception exception)
            {
                DialogErrorMessage = exception.Message;
            }
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

        private void RemoveEntityFromSystem(object itemToBeRemoved)
        {
            DialogErrorMessage = string.Empty;

            if (DeleteRegistry.TryGetValue(currentActiveEntity, out var deleteAction))
            {
                deleteAction(itemToBeRemoved);
            }

            LoadDashboardData();
            DialogVisibility = Visibility.Collapsed;
        }

        [RelayCommand]
        private void PromptDeleteRunway()
        {
            if (SelectedRunway == null)
            {
                return;
            }
            currentActiveEntity = AirportDashboardEntity.Runway;
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
            currentActiveEntity = AirportDashboardEntity.Gate;
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
            currentActiveEntity = AirportDashboardEntity.Airport;
            itemPendingDeletion = SelectedAirport;
            DeleteWarningMessage = this.ConstructDeleteWarningMessage(itemPendingDeletion);
            DeleteConfirmationVisibility = Visibility.Visible;
        }
    }
}