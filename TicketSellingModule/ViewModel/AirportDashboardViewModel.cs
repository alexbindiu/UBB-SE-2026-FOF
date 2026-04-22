using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSellingModule.Domain;
using TicketSellingModule.Service;

namespace TicketSellingModule.ViewModel
{
    public partial class AirportDashboardViewModel : ObservableObject
    {

        private readonly AirportService _airportService;
        private readonly RunwayService _runwayService;
        private readonly GateService _gateService;

        public ObservableCollection<Runway> RunwaysList { get; } = new();
        public ObservableCollection<Gate> GatesList { get; } = new();
        public ObservableCollection<Airport> AirportsList { get; } = new();

        [ObservableProperty] private Runway? _selectedRunway;
        [ObservableProperty] private Gate? _selectedGate;
        [ObservableProperty] private Airport? _selectedAirport;

        [ObservableProperty] private Visibility _dialogVisibility = Visibility.Collapsed;
        [ObservableProperty] private string _dialogTitle = string.Empty;
        [ObservableProperty] private string _dialogErrorMessage = string.Empty;
        [ObservableProperty] private Visibility _handleTimeVisibility = Visibility.Collapsed;
        [ObservableProperty] private Visibility _cityCodeVisibility = Visibility.Collapsed;

        [ObservableProperty] private int _editingId;
        [ObservableProperty] private string _editingName = string.Empty;
        [ObservableProperty] private string _editingHandleTimeText = string.Empty;
        [ObservableProperty] private string _editingCity = string.Empty;
        [ObservableProperty] private string _editingCode = string.Empty;

        [ObservableProperty] private Visibility _deleteConfirmationVisibility = Visibility.Collapsed;
        [ObservableProperty] private string _deleteWarningMessage;
        private object _itemToDelete;

        private string _currentEntity = string.Empty;

        public AirportDashboardViewModel(AirportService airportService, RunwayService runwayService, GateService gateService)
        {
            _airportService = airportService;
            _runwayService = runwayService;
            _gateService = gateService;
        }

        [RelayCommand]
        public void LoadData()
        {
            var runways = _runwayService.GetAll();
            RunwaysList.Clear();
            foreach (var r in runways) RunwaysList.Add(r);

            var gates = _gateService.GetAll();
            GatesList.Clear();
            foreach (var g in gates) GatesList.Add(g);

            var airports = _airportService.GetAll();
            AirportsList.Clear();
            foreach (var a in airports) AirportsList.Add(a);
        }

        [RelayCommand]
        private void OpenAddRunway()
        {
            _currentEntity = "Runway";
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
            if (SelectedRunway == null) return;
            _currentEntity = "Runway";
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
            _currentEntity = "Gate";
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
            if (SelectedGate == null) return;
            _currentEntity = "Gate";
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
            _currentEntity = "Airport";
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
            if (SelectedAirport == null) return;
            _currentEntity = "Airport";
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
                SaveDashboardEntity(
                    _currentEntity,
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
                _runwayService.SaveRunway(editingId, name, handleTimeText);
                return;
            }

            if (entityType == "Gate")
            {
                _gateService.SaveGate(editingId, name);
                return;
            }

            if (entityType == "Airport")
            {
                if (editingId == 0)
                    _airportService.Add(code, name, city);
                else
                    _airportService.Update(editingId, city, name, code);
                return;
            }

            throw new ArgumentException("Unsupported entity type.");
        }

        [RelayCommand]
        private void CloseDeleteDialog()
        {
            DeleteConfirmationVisibility = Visibility.Collapsed;
            _itemToDelete = null;
        }

        [RelayCommand]
        private void ConfirmDelete()
        {
            try
            {
                DeleteDashboardEntity(_itemToDelete);
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
                _runwayService.Delete(runway.Id);
                return;
            }

            if (itemToDelete is Gate gate)
            {
                _gateService.Delete(gate.Id);
                return;
            }

            if (itemToDelete is Airport airport)
            {
                _airportService.Delete(airport.Id);
                return;
            }

            throw new ArgumentException("Invalid item selected for delete.");
        }

        private string BuildDeleteWarning(object itemToDelete)
        {
            if (itemToDelete is Runway runway)
            {
                bool hasFlights = _runwayService.HasFlights(runway.Id);
                return hasFlights
                    ? $"Warning: Runway '{runway.Name}' has flights assigned. Deleting it will remove ALL associated flights. Continue?"
                    : $"Are you sure you want to delete runway '{runway.Name}'?";
            }

            if (itemToDelete is Gate gate)
            {
                bool hasFlights = _gateService.HasFlights(gate.Id);
                return hasFlights
                    ? $"Warning: Gate '{gate.Name}' has flights assigned. Deleting it will remove ALL associated flights. Continue?"
                    : $"Are you sure you want to delete gate '{gate.Name}'?";
            }

            if (itemToDelete is Airport airport)
            {
                bool hasFlights = _airportService.HasFlights(airport.Id);
                return hasFlights
                    ? $"Warning: Airport '{airport.AirportName}' has flights assigned. Deleting it will remove ALL associated flights. Continue?"
                    : $"Are you sure you want to delete airport '{airport.AirportName}'?";
            }

            throw new ArgumentException("Invalid item selected for delete.");
        }

        [RelayCommand]
        private void DeleteRunway()
        {
            if (SelectedRunway == null) return;

            _itemToDelete = SelectedRunway;
            DeleteWarningMessage = BuildDeleteWarning(_itemToDelete);
            DeleteConfirmationVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void DeleteGate()
        {
            if (SelectedGate == null) return;

            _itemToDelete = SelectedGate;
            DeleteWarningMessage = BuildDeleteWarning(_itemToDelete);
            DeleteConfirmationVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void DeleteAirport()
        {
            if (SelectedAirport == null) return;

            _itemToDelete = SelectedAirport;
            DeleteWarningMessage = BuildDeleteWarning(_itemToDelete);
            DeleteConfirmationVisibility = Visibility.Visible;
        }
    }
}
