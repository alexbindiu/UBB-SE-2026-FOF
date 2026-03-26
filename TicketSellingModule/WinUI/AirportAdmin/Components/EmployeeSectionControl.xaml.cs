using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TicketSellingModule.Domain;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.AirportAdmin.Components
{
    public sealed partial class EmployeeSectionControl : UserControl
    {
        public string SectionTitle
        {
            get => (string)GetValue(SectionTitleProperty);
            set => SetValue(SectionTitleProperty, value);
        }

        public static readonly DependencyProperty SectionTitleProperty =
            DependencyProperty.Register(
                nameof(SectionTitle),
                typeof(string),
                typeof(EmployeeSectionControl),
                new PropertyMetadata("Employees"));

        public AirportAdminViewModel? ViewModel { get; set; }

        public ObservableCollection<EmployeeRow> Items { get; } = new();

        public EmployeeSectionControl()
        {
            this.InitializeComponent();
            EmployeeListView.ItemsSource = Items;
            Loaded += EmployeeSectionControl_Loaded;
        }

        private void EmployeeSectionControl_Loaded(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            if (ViewModel == null) return;

            Items.Clear();

            var employees = ViewModel.GetAllEmployees()
                .Where(e => string.Equals(e.Role, SectionTitle, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (Employee employee in employees)
            {
                Items.Add(new EmployeeRow
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Role = employee.Role,
                    Salary = employee.Salary
                });
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            var nameBox = new TextBox { PlaceholderText = "Name" };
            var salaryBox = new TextBox { PlaceholderText = "Salary" };
            var birthdayBox = new TextBox { PlaceholderText = "Birthday (yyyy-mm-dd)" };
            var hiringDateBox = new TextBox { PlaceholderText = "Hiring date (yyyy-mm-dd)" };

            var panel = new StackPanel();
            panel.Children.Add(nameBox);
            panel.Children.Add(salaryBox);
            panel.Children.Add(birthdayBox);
            panel.Children.Add(hiringDateBox);

            var dialog = new ContentDialog
            {
                Title = $"Add {SectionTitle}",
                Content = panel,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (DateOnly.TryParse(birthdayBox.Text, out var birthday) &&
                    DateOnly.TryParse(hiringDateBox.Text, out var hiringDate) &&
                    int.TryParse(salaryBox.Text, out int salary))
                {
                    ViewModel.AddEmployee(nameBox.Text, SectionTitle, birthday, salary, hiringDate);
                    Refresh();
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            if (EmployeeListView.SelectedItem is not EmployeeRow selected) return;

            var nameBox = new TextBox { Text = selected.Name };
            var salaryBox = new TextBox { Text = selected.Salary.ToString() };

            var panel = new StackPanel();
            panel.Children.Add(nameBox);
            panel.Children.Add(salaryBox);

            var dialog = new ContentDialog
            {
                Title = $"Edit {SectionTitle}",
                Content = panel,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                int? salary = null;
                if (int.TryParse(salaryBox.Text, out int parsedSalary))
                    salary = parsedSalary;

                ViewModel.UpdateEmployee(selected.Id, nameBox.Text, SectionTitle, salary);
                Refresh();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            if (EmployeeListView.SelectedItem is not EmployeeRow selected) return;

            ViewModel.DeleteEmployee(selected.Id);
            Refresh();
        }
    }

    public class EmployeeRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public int Salary { get; set; }
    }
}