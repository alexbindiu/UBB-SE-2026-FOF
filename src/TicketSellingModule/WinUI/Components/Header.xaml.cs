using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.WinUI.Services;

namespace TicketSellingModule.WinUI.Components
{
    public sealed partial class Header : UserControl
    {
        public Header()
        {
            this.InitializeComponent();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = App.Services.GetRequiredService<INavigationService>();
            navigationService.NavigateToHome();
        }
    }
}