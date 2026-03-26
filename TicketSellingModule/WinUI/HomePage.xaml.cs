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
using Windows.Foundation;
using Windows.Foundation.Collections;
using TicketSellingModule.WinUI.AirportAdmin;


namespace TicketSellingModule.WinUI
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        private void CompanyButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SelectCompanyPage));
        }

        private void StaffButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StaffPage));
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AirportAdminPage));
        }
    }
}
