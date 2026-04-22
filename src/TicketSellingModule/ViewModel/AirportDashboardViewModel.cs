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
            CityCodeVisibility = Visibility.Visible;

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
            CityCodeVisibility = Visibility.Visible;

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

            HandleTimeVisibility = Visibility.Visible;
            CityCodeVisibility = Visibility.Visible;

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

            HandleTimeVisibility = Visibility.Visible;
            CityCodeVisibility = Visibility.Visible;

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

            HandleTimeVisibility = Visibility.Visible;
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

            HandleTimeVisibility = Visibility.Visible;
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

                if (string.IsNullOrWhiteSpace(EditingName))
                {
                    DialogErrorMessage = "Name is required.";
                    return;
                }

                if (currentEntity == "Runway")
                {
                    if (!int.TryParse(EditingHandleTimeText, out int handleTime) || handleTime < 0)
                    {
                        DialogErrorMessage = "Handle Time must be a valid positive number.";
                        return;
                    }

                    if (EditingId == 0)
                    {
                        runwayService.Add(EditingName, handleTime);
                    }
                    else
                    {
                        runwayService.Update(EditingId, EditingName, handleTime);
                    }
                }
                else if (currentEntity == "Gate")
                {
                    if (EditingId == 0)
                    {
                        gateService.Add(EditingName);
                    }
                    else
                    {
                        gateService.Update(EditingId, EditingName);
                    }
                }
                else if (currentEntity == "Airport")
                {
                    if (string.IsNullOrWhiteSpace(EditingCity) || string.IsNullOrWhiteSpace(EditingCode))
                    {
                        DialogErrorMessage = "City and Code are required.";
                        return;
                    }

                    if (EditingId == 0)
                    {
                        airportService.Add(EditingCode, EditingName, EditingCity);
                    }
                    else
                    {
                        airportService.Update(EditingId, EditingCity, EditingName, EditingCode);
                    }
                }

                LoadData();
                DialogVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DialogErrorMessage = ex.Message;
            }
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
                if (itemToDelete is Runway r)
                {
                    runwayService.Delete(r.Id);
                }
                else if (itemToDelete is Gate g)
                {
                    gateService.Delete(g.Id);
                }
                else if (itemToDelete is Airport a)
                {
                    airportService.Delete(a.Id);
                }

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

        [RelayCommand]
        private void DeleteRunway()
        {
            if (SelectedRunway == null)
            {
                return;
            }

            itemToDelete = SelectedRunway;

            bool hasFlights = runwayService.HasFlights(SelectedRunway.Id);
            DeleteWarningMessage = hasFlights
                ? $"Warning: Runway '{SelectedRunway.Name}' has flights assigned. Deleting it will remove ALL associated flights. Continue?"
                : $"Are you sure you want to delete runway '{SelectedRunway.Name}'?";

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

            bool hasFlights = gateService.HasFlights(SelectedGate.Id);
            DeleteWarningMessage = hasFlights
                ? $"Warning: Gate '{SelectedGate.Name}' has flights assigned. Deleting it will remove ALL associated flights. Continue?"
                : $"Are you sure you want to delete gate '{SelectedGate.Name}'?";

            DeleteConfirmationVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void DeleteAirport()
        {
            if (SelectedAirport == null)
            {
                return;
            }

            try
            {
                airportService.Delete(SelectedAirport.Id);
            }
            catch (Exception ex)
            {
                DialogErrorMessage = $"Cannot delete airport: {ex.Message}";
                DialogVisibility = Visibility.Visible;
                return;
            }
            LoadData();
        }
    }
}
