using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.Components
{
    public sealed partial class Header : UserControl
    {
        // Expunem ViewModel-ul pentru a-l putea accesa direct din XAML prin x:Bind
        public HeaderViewModel ViewModel { get; }

        public Header()
        {
            // Cerem ViewModel-ul care are deja NavigationService injectat
            ViewModel = App.Services.GetRequiredService<HeaderViewModel>();

            this.InitializeComponent();
        }
    }
}