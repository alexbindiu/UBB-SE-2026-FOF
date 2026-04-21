using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class HomePage : Page
    {
        // Expunem proprietatea ViewModel pentru x:Bind
        public HomeViewModel ViewModel { get; }

        public HomePage()
        {
            // Rezolvăm ViewModel-ul prin DI
            ViewModel = App.Services.GetRequiredService<HomeViewModel>();

            this.InitializeComponent();

            // Opțional: setăm DataContext pentru a menține compatibilitatea cu Bindings vechi
            this.DataContext = ViewModel;
        }
    }
}