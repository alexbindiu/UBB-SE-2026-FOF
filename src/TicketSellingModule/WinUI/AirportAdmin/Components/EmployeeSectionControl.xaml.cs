using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace TicketSellingModule.WinUI.AirportAdmin.Components
{
    public sealed partial class EmployeeSectionControl : UserControl
    {
        public EmployeeSectionControl()
        {
            this.InitializeComponent();
        }

        public string SectionTitle
        {
            get => (string)GetValue(SectionTitleProperty);
            set => SetValue(SectionTitleProperty, value);
        }
        public static readonly DependencyProperty SectionTitleProperty =
            DependencyProperty.Register(nameof(SectionTitle), typeof(string), typeof(EmployeeSectionControl), new PropertyMetadata(string.Empty));

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(EmployeeSectionControl), new PropertyMetadata(null));

        public ICommand AddCommand
        {
            get => (ICommand)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }
        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(EmployeeSectionControl), new PropertyMetadata(null));

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }
        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register(nameof(EditCommand), typeof(ICommand), typeof(EmployeeSectionControl), new PropertyMetadata(null));

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }
        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(EmployeeSectionControl), new PropertyMetadata(null));
    }
}