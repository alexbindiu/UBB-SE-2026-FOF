using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class SelectCompanyPage : Page
    {
        public SelectCompanyViewModel ViewModel { get; }

        public SelectCompanyPage()
        {
            ViewModel = App.Services.GetRequiredService<SelectCompanyViewModel>();
            this.InitializeComponent();
        }

        // Adaugă această metodă:
        private void OnCompanyClicked(object sender, ItemClickEventArgs e)
        {
            // e.ClickedItem este elementul pe care ai dat click (o instanță de Company)
            if (e.ClickedItem is Domain.Company clickedCompany)
            {
                // Executăm manual comanda din ViewModel
                ViewModel.SelectCompanyCommand.Execute(clickedCompany);
            }
        }
    }
}