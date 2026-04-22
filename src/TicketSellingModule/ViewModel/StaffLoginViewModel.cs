using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class StaffLoginViewModel : ObservableObject
    {
        private readonly EmployeeService employeeService;
        private readonly INavigationService navigationService;

        [ObservableProperty] private string employeeIdText;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private Visibility errorVisibility = Visibility.Collapsed;

        public StaffLoginViewModel(EmployeeService employeeService, INavigationService navigationService)
        {
            employeeService = employeeService;
            navigationService = navigationService;
        }

        [RelayCommand]
        private void Login()
        {
            if (!int.TryParse(EmployeeIdText, out int id))
            {
                ShowError("Invalid ID.");
                return;
            }

            var emp = employeeService.GetById(id);
            if (emp == null)
            {
                ShowError("ID was not found!");
                return;
            }

            ErrorVisibility = Visibility.Collapsed;
            ErrorMessage = string.Empty;

            navigationService.NavigateToStaffDashboard(id);
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }
    }
}