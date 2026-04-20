using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.Components
{
    public sealed partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
            DataContext = new HeaderViewModel();
        }
    }
}