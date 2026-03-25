using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class CompanyPage : Page
    {
        public CompanyViewModel ViewModel { get; }

        public CompanyPage()
        {
            this.InitializeComponent();
            ViewModel = new CompanyViewModel();

            // Initial Data Load
            ViewModel.GetAllAirports();
            AirportComboBox.ItemsSource = ViewModel.AirportsList;
            ViewModel.GetCompanyFlights(1); // Testing with Company ID 1
        }

        private async void AddFlightButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await AddFlightDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    int companyId = 1; 

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