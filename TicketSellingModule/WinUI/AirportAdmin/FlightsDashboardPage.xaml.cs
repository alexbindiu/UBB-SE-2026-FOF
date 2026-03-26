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
                FilteredFlights.Add(new FlightRow
                {
                    Id = flight.Id,
                    FlightNumber = flight.FlightNumber ?? "",
                    DateText = flight.Date.ToString("dd.MM.yyyy HH:mm"),
                    DestinationText = GetDestinationText(flight),
                    RunwayText = flight.Runway?.Name ?? "-",
                    GateText = flight.Gate?.Name ?? "-"
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
