using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class SelectCompanyViewModel : ObservableObject
    {
        private readonly ICompanyService companyService;
        private readonly INavigationService navigationService;

        public ObservableCollection<Company> Companies { get; } = new();

        public SelectCompanyViewModel(ICompanyService companyService, INavigationService navigationService)
        {
            this.companyService = companyService;
            this.navigationService = navigationService;

            LoadCompanies();
        }

        private void LoadCompanies()
        {
            Companies.Clear();
            List<Company> availableCompanies = companyService.GetAllCompanies();

            foreach (Company company in availableCompanies)
            {
                this.Companies.Add(company);
            }
        }

        [RelayCommand]
        private void SelectCompany(Company company)
        {
            if (company != null)
            {
                navigationService.NavigateToCompanyDashboard(company.Id);
            }
        }
    }
}