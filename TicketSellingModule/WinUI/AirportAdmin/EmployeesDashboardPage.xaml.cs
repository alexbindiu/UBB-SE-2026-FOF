using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TicketSellingModule.ViewModel;



namespace TicketSellingModule.WinUI.AirportAdmin
{

    public sealed partial class EmployeesDashboardPage : Page
    {
        public EmployeesDashboardViewModel ViewModel { get; private set; }

        public EmployeesDashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is EmployeesDashboardViewModel vm)
            {
                ViewModel = vm;
                DataContext = ViewModel;
                ViewModel.LoadData();
            }
        }
    }
}
