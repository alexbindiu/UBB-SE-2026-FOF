using System.Collections.ObjectModel;
using System.Windows.Input;
using TicketSellingModule.Domain;
using TicketSellingModule.Repo;
using TicketSellingModule.Service;
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public class SelectCompanyViewModel : ViewModelBase
    {
        private readonly CompanyService _companyService;
        private Company _selectedCompany;
        private readonly RelayCommand _navigateCommand;

        public SelectCompanyViewModel()
        {
            var connectionFactory = new DbConnectionFactory();
            _companyService = new CompanyService(new CompanyRepo(connectionFactory));

            Companies = new ObservableCollection<Company>();
            _navigateCommand = new RelayCommand(NavigateToDashboard, CanNavigateToDashboard);
            NavigateToDashboardCommand = _navigateCommand;
        }

        public ObservableCollection<Company> Companies { get; }

        public Company SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                if (SetProperty(ref _selectedCompany, value))
                {
                    _navigateCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand NavigateToDashboardCommand { get; }

        public void LoadCompanies()
        {
            Companies.Clear();
            foreach (var company in _companyService.GetAll())
            {
                Companies.Add(company);
            }
        }

        private bool CanNavigateToDashboard()
        {
            return SelectedCompany != null;
        }

        private void NavigateToDashboard()
        {
            if (SelectedCompany == null)
            {
                return;
            }

            NavigationService.Instance?.NavigateToCompanyDashboard(SelectedCompany.Id);
        }
    }
}
