using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class CompanyPage : Page
    {
        public CompanyViewModel ViewModel { get; }
        private int _currentCompanyId;

        public CompanyPage()
        {
            ViewModel = new CompanyViewModel();
            this.InitializeComponent();
            ViewModel.GetAllAirports();
            
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int passedCompanyId)
            {
                _currentCompanyId = passedCompanyId;

                ViewModel.GetCompanyFlights(_currentCompanyId);
            }
        }

        private void DeleteFlightButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton != null && clickedButton.Tag is int flightId)
            {
                ViewModel.DeleteFlight(flightId, _currentCompanyId);
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.SearchFlights(SearchTextBox.Text);
        }
        private async void AddFlightButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await AddFlightDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    int companyId = _currentCompanyId; 

                    string flightNum = ViewModel.GenerateFlightCode(companyId);
                    string type = (TypeComboBox.SelectedItem as string) == "Arrival" ? "ARR" : "DEP";

                    var selectedAirport = AirportComboBox.SelectedItem as TicketSellingModule.Domain.Airport;
                    int airportId = selectedAirport?.Id ?? 0;

                    int capacity = int.Parse(CapacityTextBox.Text);

                    TimeOnly depTime = TimeOnly.FromTimeSpan(DepTimePicker.Time);
                    TimeOnly arrTime = TimeOnly.FromTimeSpan(ArrTimePicker.Time);

                    bool isRecurrent = RecurrentCheckBox.IsChecked ?? false;
                    int interval = 0;
                    DateTime start = DateTime.Now;
                    DateTime end = DateTime.Now;

                    if (isRecurrent)
                    {
                        start = StartDatePicker.Date?.DateTime ?? DateTime.Now;
                        end = EndDatePicker.Date?.DateTime ?? DateTime.Now.AddMonths(1);

                        string recType = RecurrenceTypeComboBox.SelectedItem as string;
                        interval = recType switch
                        {
                            "Daily" => 1,
                            "Weekly" => 7,
                            "Monthly" => 30,
                            "Custom" => int.Parse(CustomDaysTextBox.Text),
                            _ => 0
                        };
                    }
                    else
                    {
                        start = SingleDatePicker.Date?.DateTime ?? DateTime.Now;
                        end = start;
                    }


                    ViewModel.AddFlight(flightNum, companyId, type, airportId, capacity,
                                       depTime, arrTime, interval, start, end, 1, 1);

                    ViewModel.GetCompanyFlights(1);
                    FlightsListView.ItemsSource = ViewModel.CompanyFlightsList; // Force UI refresh


                    ViewModel.GetCompanyFlights(companyId);
                }
                catch (Exception ex)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Error Saving Flight",
                        Content = "Please ensure all fields are filled correctly: " + ex.Message,
                        CloseButtonText = "Ok",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        private void RecurrentCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SingleDatePicker.Visibility = Visibility.Collapsed;
            RecurrentPanel.Visibility = Visibility.Visible;
        }

        private void RecurrentCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SingleDatePicker.Visibility = Visibility.Visible;
            RecurrentPanel.Visibility = Visibility.Collapsed;
        }

        private void RecurrenceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecurrenceTypeComboBox.SelectedItem is string selectedType && selectedType == "Custom")
            {
                CustomDaysTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                CustomDaysTextBox.Visibility = Visibility.Collapsed;
            }
        }
        
    }
}