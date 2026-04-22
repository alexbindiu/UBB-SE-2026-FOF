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

        [ObservableProperty] private Visibility _dialogVisibility = Visibility.Visible;
        [ObservableProperty] private string _dialogTitle = string.Empty;
        [ObservableProperty] private string _dialogErrorMessage = string.Empty;
        [ObservableProperty] private Visibility _handleTimeVisibility = Visibility.Visible;
        [ObservableProperty] private Visibility _cityCodeVisibility = Visibility.Visible;

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
            DialogVisibility = Visibility.Visible;
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

                if (_currentEntity == "Runway")
                {
                    if (!int.TryParse(EditingHandleTimeText, out int handleTime) || handleTime < 0)
                    {
                        DialogErrorMessage = "Handle Time must be a valid positive number.";
                        return;
                    }

                    if (EditingId == 0) _runwayService.Add(EditingName, handleTime);
                    else _runwayService.Update(EditingId, EditingName, handleTime);
                }
                else if (_currentEntity == "Gate")
                {
                    if (EditingId == 0) _gateService.Add(EditingName);
                    else _gateService.Update(EditingId, EditingName);
                }
                else if (_currentEntity == "Airport")
                {
                    if (string.IsNullOrWhiteSpace(EditingCity) || string.IsNullOrWhiteSpace(EditingCode))
                    {
                        DialogErrorMessage = "City and Code are required.";
                        return;
                    }

                    if (EditingId == 0) _airportService.Add(EditingCode, EditingName, EditingCity);
                    else _airportService.Update(EditingId, EditingCity, EditingName, EditingCode);
                }

                
                LoadData();
                DialogVisibility = Visibility.Visible;
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
            _itemToDelete = null;
        }

        [RelayCommand]
        private void ConfirmDelete()
        {
            try
            {
                if (_itemToDelete is Runway r)
                {
                    _runwayService.Delete(r.Id);
                }
                else if (_itemToDelete is Gate g)
                {
                    _gateService.Delete(g.Id);
                }
                else if (_itemToDelete is Airport a)
                {
                    _airportService.Delete(a.Id);
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
            if (SelectedRunway == null) return;

            _itemToDelete = SelectedRunway;

            bool hasFlights = _runwayService.HasFlights(SelectedRunway.Id);
            DeleteWarningMessage = hasFlights
                ? $"Warning: Runway '{SelectedRunway.Name}' has flights assigned. Deleting it will remove ALL associated flights. Continue?"
                : $"Are you sure you want to delete runway '{SelectedRunway.Name}'?";

            DeleteConfirmationVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void DeleteGate()
        {
            if (SelectedGate == null) return;

            _itemToDelete = SelectedGate;

            bool hasFlights = _gateService.HasFlights(SelectedGate.Id);
            DeleteWarningMessage = hasFlights
                ? $"Warning: Gate '{SelectedGate.Name}' has flights assigned. Deleting it will remove ALL associated flights. Continue?"
                : $"Are you sure you want to delete gate '{SelectedGate.Name}'?";

            DeleteConfirmationVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private void DeleteAirport()
        {
            if (SelectedAirport == null) return;
            try
            {
                _airportService.Delete(SelectedAirport.Id);
            }
            catch(Exception ex)
            {
                DialogErrorMessage = $"Cannot delete airport: {ex.Message}";
                DialogVisibility = Visibility.Visible;
                return;
            }
            LoadData();
        }
    }
}
