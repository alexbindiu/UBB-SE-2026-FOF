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
        public AirportDashboardViewModel ViewModel { get; set; }

        public AirportDashboardPage()
        {
            this.InitializeComponent();
            //DataContext = _viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is AirportDashboardViewModel vm)
            {
                ViewModel = vm;
                this.DataContext = ViewModel;
                ViewModel.LoadData();
            }
        }
    }
}
        

              