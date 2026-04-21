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


namespace TicketSellingModule.WinUI.AirportAdmin
{

    public sealed partial class EmployeesDashboardPage : Page
    {
        public EmployeesDashboardViewModel ViewModel;

        public EmployeesDashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is EmployeesDashboardViewModel vm)
            {
                ViewModel = vm;
                this.DataContext = ViewModel;

                ViewModel.LoadData();
            }
        }
    }
}
