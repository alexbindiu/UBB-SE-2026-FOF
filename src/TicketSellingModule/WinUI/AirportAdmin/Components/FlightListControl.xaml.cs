using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
                new PropertyMetadata(null));

        public FlightRow? SelectedItem
        {
            get => (FlightRow?)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(FlightRow),
                typeof(FlightListControl),
                new PropertyMetadata(null));

        public FlightListControl()
        {
            this.InitializeComponent();
        }
    }
}
