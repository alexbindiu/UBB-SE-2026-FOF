using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TicketSellingModule.Repo;
using TicketSellingModule.Service;
using TicketSellingModule.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TicketSellingModule.WinUI.AirportAdmin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AirportAdminPage : Page         //NAVIGATIA DIN STANGA
    {
        private readonly AirportAdminViewModel _viewModel;
        private readonly EmployeesDashboardViewModel _employeesViewModel;

        public AirportAdminPage()
        {
            this.InitializeComponent();
            _viewModel = new AirportAdminViewModel();
            var connectionFactory = new DbConnectionFactory();
            _employeesViewModel = new EmployeesDashboardViewModel(
                new EmployeeService(new EmployeeRepo(connectionFactory)));

            Loaded += AirportAdminPage_Loaded;
        }

        private void AirportAdminPage_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(FlightsDashboardPage), _viewModel);
            HighlightSelectedButton(FlightsButton);
        }

        private void FlightsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(FlightsDashboardPage), _viewModel);
            HighlightSelectedButton(FlightsButton);
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(EmployeesDashboardPage), _employeesViewModel);
            HighlightSelectedButton(EmployeesButton);
        }

        private void AirportButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(AirportDashboardPage), _viewModel);
            HighlightSelectedButton(AirportButton);
        }

        private void HighlightSelectedButton(Button selectedButton)
        {
            FlightsButton.Background = null;
            EmployeesButton.Background = null;
            AirportButton.Background = null;

            selectedButton.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.LightGray);
        }
    }
}
