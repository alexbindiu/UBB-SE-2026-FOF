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
        private const string ErrorMessageFailedLogin = "Failed Login";

        [ObservableProperty] private string employeeIdText;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private Visibility errorVisibility = Visibility.Collapsed;

        [RelayCommand]
        private void Login()
        {
            try
            {
                int employeeId = employeeService.Login(EmployeeIdText);

                ErrorVisibility = Visibility.Collapsed;
                ErrorMessage = string.Empty;

                navigationService.NavigateToStaffDashboard(employeeId);
            }
            catch (Exception exception)
            {
                ShowError(ErrorMessageFailedLogin + ": " + exception.Message);
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }
    }
}