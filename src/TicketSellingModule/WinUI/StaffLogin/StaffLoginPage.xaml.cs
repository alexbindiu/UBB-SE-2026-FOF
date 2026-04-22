using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.StaffLogin
{
    public sealed partial class StaffLoginPage : Page
    {
        public StaffLoginViewModel ViewModel { get; private set; } = null!;

        public StaffLoginPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is StaffLoginViewModel vm)
            {
                ViewModel = vm;
                DataContext = ViewModel;
            }
        }
    }
}