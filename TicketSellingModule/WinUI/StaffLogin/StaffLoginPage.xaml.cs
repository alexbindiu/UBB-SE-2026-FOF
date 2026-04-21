using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.StaffLogin
{
    public sealed partial class StaffLoginPage : Page
    {
        public StaffLoginViewModel ViewModel { get; }

        public StaffLoginPage()
        {
            InitializeComponent();
            ViewModel = new StaffLoginViewModel();
            ViewModel.LoginSucceeded += OnLoginSucceeded;
            DataContext = ViewModel;
        }

        private void OnLoginSucceeded(int employeeId)
        {
            Frame.Navigate(typeof(TicketSellingModule.WinUI.StaffPage), employeeId);
        }
    }
}