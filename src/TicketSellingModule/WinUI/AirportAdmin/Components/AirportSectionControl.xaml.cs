using System.Windows.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

        public object Items
        {
            get => GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(
                nameof(Items),
                typeof(object),
                typeof(AirportSectionControl),
                new PropertyMetadata(null, OnItemsChanged));

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AirportSectionControl control)
            {
                control.ListViewControl.ItemsSource = e.NewValue;
            }
        }

        public AirportSectionControl()
        {
            this.InitializeComponent();
        }

        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(AirportSectionControl),
                new PropertyMetadata(null));

        public ICommand? AddCommand
        {
            get => (ICommand?)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }

        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register(
                nameof(AddCommand),
                typeof(ICommand),
                typeof(AirportSectionControl),
                new PropertyMetadata(null));

        public ICommand? EditCommand
        {
            get => (ICommand?)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register(
                nameof(EditCommand),
                typeof(ICommand),
                typeof(AirportSectionControl),
                new PropertyMetadata(null));

        public ICommand? DeleteCommand
        {
            get => (ICommand?)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(
                nameof(DeleteCommand),
                typeof(ICommand),
                typeof(AirportSectionControl),
                new PropertyMetadata(null));

        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                nameof(DisplayMemberPath),
                typeof(string),
                typeof(AirportSectionControl),
                new PropertyMetadata(string.Empty, OnDisplayMemberPathChanged));

        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AirportSectionControl control)
            {
                control.ListViewControl.DisplayMemberPath = e.NewValue as string ?? string.Empty;
            }
        }
    }
}