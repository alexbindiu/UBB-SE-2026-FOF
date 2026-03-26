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
    public sealed partial class EmployeesDashboardPage : Page
    {
        private AirportAdminViewModel _viewModel;

        public EmployeesDashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _viewModel = (AirportAdminViewModel)e.Parameter;

            AirTrafficSection.ViewModel = _viewModel;
            FlightDispatchersSection.ViewModel = _viewModel;
            GroundHandlersSection.ViewModel = _viewModel;
            EngineersSection.ViewModel = _viewModel;

            AirTrafficSection.Refresh();
            FlightDispatchersSection.Refresh();
            GroundHandlersSection.Refresh();
            EngineersSection.Refresh();
        }
    }
}
