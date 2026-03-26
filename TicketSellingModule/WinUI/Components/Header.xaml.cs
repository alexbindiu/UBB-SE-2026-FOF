using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace TicketSellingModule.WinUI.Components
{
    public sealed partial class Header : UserControl
    {
        public Header()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Find the parent Frame and navigate to HomePage
            var parent = this.Parent;
            while (parent != null && parent is not Frame)
                parent = (parent as FrameworkElement)?.Parent;

            if (parent is Frame frame)
                frame.Navigate(typeof(HomePage));
        }
    }
}