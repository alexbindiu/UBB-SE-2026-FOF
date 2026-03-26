using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace TicketSellingModule.WinUI.AirportAdmin.Components
{
    public sealed partial class AirportSectionControl : UserControl
    {
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(AirportSectionControl),
                new PropertyMetadata("Title"));

        public ObservableCollection<object> Items
        {
            get => (ObservableCollection<object>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(
                nameof(Items),
                typeof(ObservableCollection<object>),
                typeof(AirportSectionControl),
                new PropertyMetadata(null, OnItemsChanged));

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AirportSectionControl control)
            {
                control.ListViewControl.ItemsSource = e.NewValue as ObservableCollection<object>;
            }
        }

        public event RoutedEventHandler? AddClicked;
        public event RoutedEventHandler? EditClicked;
        public event RoutedEventHandler? DeleteClicked;

        public AirportSectionControl()
        {
            this.InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AddClicked?.Invoke(this, e);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            EditClicked?.Invoke(this, e);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteClicked?.Invoke(this, e);
        }

        public object? SelectedItem => ListViewControl.SelectedItem;
    }
}