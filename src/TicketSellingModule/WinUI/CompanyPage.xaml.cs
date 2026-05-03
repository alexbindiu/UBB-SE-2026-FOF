using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class CompanyPage : Page
    {
        private const string DefaultErrorTitle = "Error Saving Flight";
        private const string DefaultErrorContent = "Please ensure all fields are filled correctly: ";
        private const string DefaultErrorCloseButtonText = "Ok";

        public CompanyViewModel ViewModel { get; private set; } = null!;

        public CompanyPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<CompanyViewModel, int> context)
            {
                ViewModel = context.Item1;
                DataContext = ViewModel;
                ViewModel.RefreshGatesList();
                ViewModel.RefreshRunwaysList();
                ViewModel.InitializeCompanyDashboard(context.Item2);
            }
        }

        private async void AddFlightButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ResetInputFields();
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
                    Title = DefaultErrorTitle,
                    Content = DefaultErrorContent + ex.Message,
                    CloseButtonText = DefaultErrorCloseButtonText,
                    XamlRoot = XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}