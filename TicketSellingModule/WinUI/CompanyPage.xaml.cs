using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class CompanyPage : Page
    {
        public CompanyViewModel ViewModel { get; }

        public CompanyPage()
        {
            ViewModel = App.Services.GetRequiredService<CompanyViewModel>();
            InitializeComponent();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int passedCompanyId)
            {
                ViewModel.InitializeCompany(passedCompanyId);
            }
        }

        private async void AddFlightButton_Click(object sender, RoutedEventArgs e)
        {
            await AddFlightDialog.ShowAsync();
        }

        private async void AddFlightDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                ViewModel.AddFlightFromInputs();
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                sender.Hide();
                var errorDialog = new ContentDialog
                {
                    Title = "Error Saving Flight",
                    Content = "Please ensure all fields are filled correctly: " + ex.Message,
                    CloseButtonText = "Ok",
                    XamlRoot = XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}