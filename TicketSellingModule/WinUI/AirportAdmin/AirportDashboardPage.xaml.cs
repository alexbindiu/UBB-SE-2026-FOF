using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using TicketSellingModule.Domain;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.AirportAdmin
{
    public sealed partial class AirportDashboardPage : Page
    {
        private AirportAdminViewModel _viewModel;

        public AirportDashboardPage()
        {
            this.InitializeComponent();
            DataContext = _viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _viewModel = (AirportAdminViewModel)e.Parameter;
            DataContext = _viewModel;
        }
    }
}
        

       