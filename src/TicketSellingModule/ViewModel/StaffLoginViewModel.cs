using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class StaffLoginViewModel : ObservableObject
    {
        private readonly EmployeeService _employeeService;
        private readonly INavigationService _navigationService;

        [ObservableProperty] private string _employeeIdText;
        [ObservableProperty] private string _errorMessage;
        [ObservableProperty] private Visibility _errorVisibility = Visibility.Collapsed;

        public StaffLoginViewModel(EmployeeService employeeService, INavigationService navigationService)
        {
            _employeeService = employeeService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void Login()
        {
            if (!int.TryParse(EmployeeIdText, out int id))
            {
                ShowError("Invalid ID.");
                return;
            }

            var emp = _employeeService.GetById(id);
            if (emp == null)
            {
                ShowError("ID was not found!");
                return;
            }

            ErrorVisibility = Visibility.Collapsed;
            ErrorMessage = string.Empty;

            
            _navigationService.NavigateToStaffDashboard(id);
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }
    }
}