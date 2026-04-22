using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI
{
    public sealed partial class StaffPage : Page
    {
        public StaffPageViewModel ViewModel { get; private set; } = null!;

        public StaffPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<StaffPageViewModel, int> context)
            {
                ViewModel = context.Item1;
                DataContext = ViewModel;
                ViewModel.Initialize(context.Item2);
            }
        }
    }
}