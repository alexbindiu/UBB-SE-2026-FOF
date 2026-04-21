using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.StaffLogin
{
    public sealed partial class StaffLoginPage : Page
    {
        public StaffLoginViewModel ViewModel { get; }

        public StaffLoginPage()
        {
            // Rezolvăm prin DI (care injectează automat EmployeeService și NavigationService)
            ViewModel = App.Services.GetRequiredService<StaffLoginViewModel>();
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }
    }
}