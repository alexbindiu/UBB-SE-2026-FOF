using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class StaffLoginViewModel(
        IEmployeeService employeeService,
        INavigationService navigationService) : INotifyPropertyChanged
    {
        private const string ErrorMessageFailedLogin = "Failed Login";

        public event PropertyChangedEventHandler PropertyChanged;

        private string employeeIdText;

        public string EmployeeIdText
        {
            get => employeeIdText;
            set
            {
                if (employeeIdText != value)
                {
                    employeeIdText = value;
                    OnPropertyChanged();
                }
            }
        }

        private string errorMessage;

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (errorMessage != value)
                {
                    errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility errorVisibility = Visibility.Collapsed;

        public Visibility ErrorVisibility
        {
            get => errorVisibility;
            set
            {
                if (errorVisibility != value)
                {
                    errorVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}