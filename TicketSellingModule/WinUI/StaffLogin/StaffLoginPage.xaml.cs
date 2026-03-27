using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.StaffLogin
{
    public sealed partial class StaffLoginPage : Page
    {
        private readonly EmployeeViewModel _viewModel;

        public StaffLoginPage()
        {
            this.InitializeComponent();
            _viewModel = new EmployeeViewModel();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(EmployeeIdTextBox.Text, out int id))
            {
                var emp = _viewModel.GetEmployeeInfo(id);
                if (emp != null)

                {
                    Frame.Navigate(typeof(TicketSellingModule.WinUI.StaffPage), id);
                }
                else
                {
                    ErrorTextBlock.Text = "ID-ul nu a fost găsit!";
                    ErrorTextBlock.Visibility = Visibility.Visible;
                }
            }
        }
    }
}