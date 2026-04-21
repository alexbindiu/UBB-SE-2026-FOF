using System;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using TicketSellingModule.Repo;
using TicketSellingModule.Service;

namespace TicketSellingModule.ViewModel
{
    public class StaffLoginViewModel : ViewModelBase
    {
        private readonly EmployeeService _employeeService;
        private string _employeeIdText;
        private string _errorMessage;
        private Visibility _errorVisibility = Visibility.Collapsed;

        public StaffLoginViewModel()
        {
            var connectionFactory = new DbConnectionFactory();
            _employeeService = new EmployeeService(new EmployeeRepo(connectionFactory));

            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        public string EmployeeIdText
        {
            get => _employeeIdText;
            set => SetProperty(ref _employeeIdText, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        public Visibility ErrorVisibility
        {
            get => _errorVisibility;
            private set => SetProperty(ref _errorVisibility, value);
        }

        public ICommand LoginCommand { get; }

        public event Action<int> LoginSucceeded;

        private void ExecuteLogin()
        {
            if (!int.TryParse(EmployeeIdText, out int id))
            {
                ShowError("Invalid ID.");
                return;
            }

            var emp = _employeeService.GetById(id);
            if (emp == null)
            {
                ShowError("ID not found!");
                return;
            }

            ErrorVisibility = Visibility.Collapsed;
            ErrorMessage = string.Empty;
            LoginSucceeded?.Invoke(id);
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }
    }
}
