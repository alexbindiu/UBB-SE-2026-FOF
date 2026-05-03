using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.Input;

using TicketSellingModule.Data.Domain;
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class SelectCompanyViewModel : INotifyPropertyChanged
    {
        private readonly ICompanyService companyService;
        private readonly INavigationService navigationService;

        private ObservableCollection<Company> companies;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Company> Companies
        {
            get => companies;
            set
            {
                if (companies != value)
                {
                    companies = value;
                    OnPropertyChanged();
                }
            }
        }

        public SelectCompanyViewModel(ICompanyService companyService, INavigationService navigationService)
        {
            this.companyService = companyService;
            this.navigationService = navigationService;

            LoadCompanies();
        }

        private void LoadCompanies()
        {
            List<Company> availableCompanies = companyService.GetAllCompanies();
            Companies = new ObservableCollection<Company>(availableCompanies);
        }

        public IRelayCommand SelectCompanyCommand => new RelayCommand<Company>(SelectCompany);
        private void SelectCompany(Company company)
        {
            if (company != null)
            {
                navigationService.NavigateToCompanyDashboard(company.Id);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}