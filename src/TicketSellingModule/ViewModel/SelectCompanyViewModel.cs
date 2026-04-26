using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.ViewModel
{
    public partial class SelectCompanyViewModel : ObservableObject
    {
        private readonly ICompanyService companyService;
        private readonly INavigationService navigationService;

        [ObservableProperty] private ObservableCollection<Company> companies;

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