using Microsoft.UI.Xaml;
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
            ViewModel = new StaffPageViewModel();
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