using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class StaffPage : Page
    {
        public StaffPageViewModel ViewModel { get; }

        public StaffPage()
        {
            // Request the fully constructed ViewModel from Dependency Injection
            ViewModel = App.Services.GetRequiredService<StaffPageViewModel>();
            InitializeComponent();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            int employeeId = e.Parameter is int id ? id : 0;
            ViewModel.Initialize(employeeId);
        }
    }
}