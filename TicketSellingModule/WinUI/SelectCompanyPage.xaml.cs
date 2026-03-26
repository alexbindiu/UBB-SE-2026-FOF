using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.ViewModel;
using TicketSellingModule.Domain;

namespace TicketSellingModule.WinUI
{
    public sealed partial class SelectCompanyPage : Page
    {
        public CompanyViewModel ViewModel { get; }

        public SelectCompanyPage()
        {
            this.InitializeComponent();
            ViewModel = new CompanyViewModel();

            // Load the companies from your DB
            ViewModel.GetAllCompanies();
            CompaniesComboBox.ItemsSource = ViewModel.CompaniesList;
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Check if they actually selected a company
            if (CompaniesComboBox.SelectedItem is Company selectedCompany)
            {
                // 2. Navigate to the Dashboard, and pass the ID as the "Parameter"
                this.Frame.Navigate(typeof(CompanyPage), selectedCompany.Id);
            }
        }
    }
}