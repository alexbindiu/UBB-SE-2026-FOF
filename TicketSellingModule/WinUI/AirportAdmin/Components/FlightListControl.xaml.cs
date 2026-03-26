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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TicketSellingModule.WinUI.AirportAdmin.Components
{
    public sealed partial class FlightListControl : UserControl
    {
        public ObservableCollection<FlightRow> Items
        {
            get => (ObservableCollection<FlightRow>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(
                nameof(Items),
                typeof(ObservableCollection<FlightRow>),
                typeof(FlightListControl),
                new PropertyMetadata(null, OnItemsChanged));

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FlightListControl control)
            {
                control.FlightsListView.ItemsSource = e.NewValue as ObservableCollection<FlightRow>;
            }
        }

        public FlightListControl()
        {
            this.InitializeComponent();
        }
    }
}
