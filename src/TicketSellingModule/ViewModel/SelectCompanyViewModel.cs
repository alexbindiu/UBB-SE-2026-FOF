using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class SelectCompanyViewModel : ObservableObject
    {
        private readonly CompanyService companyService;
        private readonly INavigationService navigationService;

        public ObservableCollection<Company> Companies { get; } = new();

        public SelectCompanyViewModel(CompanyService companyService, INavigationService navigationService)
        {
            this.companyService = companyService;
            this.navigationService = navigationService;

            LoadCompanies();
        }

        private void LoadCompanies()
        {
            Companies.Clear();
            var list = companyService.GetAll();
            foreach (var company in list)
            {
                Companies.Add(company);
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