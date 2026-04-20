using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class SelectCompanyPage : Page
    {
        public SelectCompanyViewModel ViewModel { get; }

        public SelectCompanyPage()
        {
            InitializeComponent();
            ViewModel = new SelectCompanyViewModel();
            DataContext = ViewModel;
            ViewModel.LoadCompanies();
        }
    }
}
