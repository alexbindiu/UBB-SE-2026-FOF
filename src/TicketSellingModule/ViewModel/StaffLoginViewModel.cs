using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class StaffLoginViewModel(
        IEmployeeService employeeService,
        INavigationService navigationService) : ObservableObject
    {

        [ObservableProperty] private string employeeIdText;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private Visibility errorVisibility = Visibility.Collapsed;

        [RelayCommand]
        private void Login()
        {
            if (!int.TryParse(EmployeeIdText, out int employeeId))
            {
                ShowError("Invalid ID.");
                return;
            }

            var emp = employeeService.GetEmployeeById(employeeId);
            if (emp == null)
            {
                ShowError("ID was not found!");
                return;
            }

            ErrorVisibility = Visibility.Collapsed;
            ErrorMessage = string.Empty;

            navigationService.NavigateToStaffDashboard(employeeId);
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }
    }
}