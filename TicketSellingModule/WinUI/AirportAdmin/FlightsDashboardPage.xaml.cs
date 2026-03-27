using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TicketSellingModule.Domain;
using TicketSellingModule.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TicketSellingModule.WinUI.AirportAdmin.Components;


namespace TicketSellingModule.WinUI.AirportAdmin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FlightsDashboardPage : Page
    {
        private AirportAdminViewModel _viewModel;

        private List<Flight> _allFlights = new();

        public ObservableCollection<FlightRow> FilteredFlights { get; } = new();

        public FlightsDashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _viewModel = (AirportAdminViewModel)e.Parameter;
            LoadFlights();
        }

        private void LoadFlights()
        {
            _allFlights = _viewModel.GetAllFlights();
            ApplyFilter(FlightSearchBox.Text);
        }

        private void ApplyFilter(string? query)
        {
            string text = query?.Trim().ToLower() ?? string.Empty;

            var filtered = string.IsNullOrWhiteSpace(text)
                ? _allFlights
                : _allFlights.Where(f =>
                       (f.FlightNumber?.ToLower().Contains(text) ?? false) ||
                       (f.Date.ToString("dd.MM.yyyy HH:mm").ToLower().Contains(text)) ||
                       (GetDestinationText(f).ToLower().Contains(text)) ||
                       (f.Runway?.Name?.ToLower().Contains(text) ?? false) ||
                       (f.Gate?.Name?.ToLower().Contains(text) ?? false))
                    .ToList();

            FilteredFlights.Clear();

            foreach (var flight in filtered)
            {
                var crew = _viewModel.GetFlightCrew(flight.Id);
                string crewNames = crew.Count > 0 ? string.Join(", ", crew.Select(c => c.Name)) : "Unassigned";

                FilteredFlights.Add(new FlightRow
                {
                    Id = flight.Id,
                    FlightNumber = flight.FlightNumber ?? "",
                    DateText = flight.Date.ToString("dd.MM.yyyy HH:mm"),
                    DestinationText = GetDestinationText(flight),
                    RunwayText = flight.Runway?.Name ?? "-",
                    GateText = flight.Gate?.Name ?? "-",
                    CrewText = crewNames // <-- Add the Crew names here!
                });
            }
        }

        private string GetDestinationText(Flight flight)
        {
            if (flight.Route == null)
                return "-";

            // adjust this if your Route model uses another property name
            var destinationAirport = flight.Route.Airport;
            if (destinationAirport == null)
                return "-";

            return $"{destinationAirport.AirportCode} - {destinationAirport.AirportName}";
        }

        private async void AssignCrewButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = FlightListControl.SelectedItem;
            if (selectedRow == null)
            {
                await new ContentDialog
                {
                    Title = "Action Denied",
                    Content = "Please select a flight from the table first.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
                return;
            }

            var flight = _viewModel.GetFlightById(selectedRow.Id);
            var availableEmployees = _viewModel.GetAvailableEmployeesForFlight(flight);

            // NEW: Get the crew that is ALREADY assigned to this flight
            var currentCrewIds = _viewModel.GetFlightCrew(flight.Id).Select(c => c.Id).ToList();

            var stackPanel = new StackPanel { Spacing = 10 };
            var errorText = new TextBlock
            {
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
                Visibility = Visibility.Collapsed
            };
            stackPanel.Children.Add(errorText);

            var checkBoxes = new List<CheckBox>();

            foreach (var group in availableEmployees.GroupBy(emp => emp.Role))
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = group.Key,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                });

                foreach (var emp in group)
                {
                    var cb = new CheckBox { Content = emp.Name, Tag = emp };

                    // NEW: If they are already assigned to this flight, check the box automatically!
                    if (currentCrewIds.Contains(emp.Id))
                    {
                        cb.IsChecked = true;
                    }

                    checkBoxes.Add(cb);
                    stackPanel.Children.Add(cb);
                }
            }

            var dialog = new ContentDialog
            {
                Title = $"Manage Crew: {selectedRow.FlightNumber}",
                Content = new ScrollViewer { Content = stackPanel, MaxHeight = 400 },
                PrimaryButtonText = "Save Assignments",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            dialog.Closing += (s, args) =>
            {
                if (args.Result == ContentDialogResult.Primary)
                {
                    var selectedCount = checkBoxes.Count(cb => cb.IsChecked == true);
                    if (selectedCount < 4)
                    {
                        args.Cancel = true;
                        errorText.Text = "You must select at least 4 employees to operate a flight.";
                        errorText.Visibility = Visibility.Visible;
                    }
                }
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var selectedIds = checkBoxes.Where(c => c.IsChecked == true)
                                            .Select(c => ((Employee)c.Tag).Id).ToList();

                // NEW: Call the smart update method instead of the old assign method
                _viewModel.UpdateCrewForFlight(flight.Id, selectedIds);
                LoadFlights();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadFlights();
        }

        private void FlightSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ApplyFilter(sender.Text);
        }

        private void FlightSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ApplyFilter(sender.Text);
        }


    }
}
