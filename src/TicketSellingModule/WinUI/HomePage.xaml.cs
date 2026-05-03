using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class HomePage : Page
    {
        public HomeViewModel ViewModel { get; private set; } = null!;

        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is HomeViewModel vm)
            {
                ViewModel = vm;
                DataContext = ViewModel;
                Bindings.Update();
            }
        }
    }
}