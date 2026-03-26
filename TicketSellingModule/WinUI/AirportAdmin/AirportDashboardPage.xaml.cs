using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using TicketSellingModule.Domain;
using TicketSellingModule.ViewModel;

namespace TicketSellingModule.WinUI.AirportAdmin
{
    public sealed partial class AirportDashboardPage : Page
    {
        private AirportAdminViewModel _viewModel;

        public AirportDashboardPage()
        {
            this.InitializeComponent();

            RunwaysSection.Items = new ObservableCollection<object>();
            GatesSection.Items = new ObservableCollection<object>();
            AirportsSection.Items = new ObservableCollection<object>();

            RunwaysSection.AddClicked += AddRunway_Click;
            RunwaysSection.EditClicked += EditRunway_Click;
            RunwaysSection.DeleteClicked += DeleteRunway_Click;

            GatesSection.AddClicked += AddGate_Click;
            GatesSection.EditClicked += EditGate_Click;
            GatesSection.DeleteClicked += DeleteGate_Click;

            AirportsSection.AddClicked += AddAirport_Click;
            AirportsSection.EditClicked += EditAirport_Click;
            AirportsSection.DeleteClicked += DeleteAirport_Click;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _viewModel = (AirportAdminViewModel)e.Parameter;
            RefreshAll();
        }

        private void RefreshAll()
        {
            LoadRunways();
            LoadGates();
            LoadAirports();
        }

        private void LoadRunways()
        {
            var list = (ObservableCollection<object>)RunwaysSection.Items;
            list.Clear();

            foreach (Runway runway in _viewModel.GetAllRunways())
            {
                list.Add(new SimpleItemRow
                {
                    Id = runway.Id,
                    MainValue = runway.Name,
                    SecondaryValue = runway.HandleTime.ToString(),
                    DisplayText = $"{runway.Name} | Handle time: {runway.HandleTime}"
                });
            }
        }

        private void LoadGates()
        {
            var list = (ObservableCollection<object>)GatesSection.Items;
            list.Clear();

            foreach (Gate gate in _viewModel.GetAllGates())
            {
                list.Add(new SimpleItemRow
                {
                    Id = gate.Id,
                    MainValue = gate.Name,
                    DisplayText = gate.Name
                });
            }
        }

        private void LoadAirports()
        {
            var list = (ObservableCollection<object>)AirportsSection.Items;
            list.Clear();

            foreach (Airport airport in _viewModel.GetAllAirports())
            {
                list.Add(new SimpleItemRow
                {
                    Id = airport.Id,
                    MainValue = airport.AirportName,
                    SecondaryValue = airport.City,
                    ThirdValue = airport.AirportCode,
                    DisplayText = $"{airport.AirportCode} | {airport.AirportName} | {airport.City}"
                });
            }
        }

        private async void AddRunway_Click(object sender, RoutedEventArgs e)
        {
            var nameBox = new TextBox { PlaceholderText = "Runway name" };
            var handleTimeBox = new TextBox { PlaceholderText = "Handle time" };

            var panel = new StackPanel();
            panel.Children.Add(nameBox);
            panel.Children.Add(handleTimeBox);

            var dialog = CreateDialog("Add Runway", panel);

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if (int.TryParse(handleTimeBox.Text, out int handleTime))
                {
                    _viewModel.AddRunway(nameBox.Text, handleTime);
                    LoadRunways();
                }
            }
        }

        private async void EditRunway_Click(object sender, RoutedEventArgs e)
        {
            if (RunwaysSection.SelectedItem is not SimpleItemRow selected) return;

            var nameBox = new TextBox { Text = selected.MainValue };
            var handleTimeBox = new TextBox { Text = selected.SecondaryValue };

            var panel = new StackPanel();
            panel.Children.Add(nameBox);
            panel.Children.Add(handleTimeBox);

            var dialog = CreateDialog("Edit Runway", panel);

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if (int.TryParse(handleTimeBox.Text, out int handleTime))
                {
                    _viewModel.UpdateRunway(selected.Id, nameBox.Text, handleTime);
                    LoadRunways();
                }
            }
        }

        private void DeleteRunway_Click(object sender, RoutedEventArgs e)
        {
            if (RunwaysSection.SelectedItem is not SimpleItemRow selected) return;

            _viewModel.DeleteRunway(selected.Id);
            LoadRunways();
        }

        private async void AddGate_Click(object sender, RoutedEventArgs e)
        {
            var nameBox = new TextBox { PlaceholderText = "Gate name" };
            var dialog = CreateDialog("Add Gate", nameBox);

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                _viewModel.AddGate(nameBox.Text);
                LoadGates();
            }
        }

        private async void EditGate_Click(object sender, RoutedEventArgs e)
        {
            if (GatesSection.SelectedItem is not SimpleItemRow selected) return;

            var nameBox = new TextBox { Text = selected.MainValue };
            var dialog = CreateDialog("Edit Gate", nameBox);

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                _viewModel.UpdateGate(selected.Id, nameBox.Text);
                LoadGates();
            }
        }

        private void DeleteGate_Click(object sender, RoutedEventArgs e)
        {
            if (GatesSection.SelectedItem is not SimpleItemRow selected) return;

            _viewModel.DeleteGate(selected.Id);
            LoadGates();
        }

        private async void AddAirport_Click(object sender, RoutedEventArgs e)
        {
            var codeBox = new TextBox { PlaceholderText = "Airport code" };
            var nameBox = new TextBox { PlaceholderText = "Airport name" };
            var cityBox = new TextBox { PlaceholderText = "City" };

            var panel = new StackPanel();
            panel.Children.Add(codeBox);
            panel.Children.Add(nameBox);
            panel.Children.Add(cityBox);

            var dialog = CreateDialog("Add Airport", panel);

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                _viewModel.AddAirport(codeBox.Text, nameBox.Text, cityBox.Text);
                LoadAirports();
            }
        }

        private async void EditAirport_Click(object sender, RoutedEventArgs e)
        {
            if (AirportsSection.SelectedItem is not SimpleItemRow selected) return;

            var codeBox = new TextBox { Text = selected.ThirdValue };
            var nameBox = new TextBox { Text = selected.MainValue };
            var cityBox = new TextBox { Text = selected.SecondaryValue };

            var panel = new StackPanel();
            panel.Children.Add(codeBox);
            panel.Children.Add(nameBox);
            panel.Children.Add(cityBox);

            var dialog = CreateDialog("Edit Airport", panel);

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                _viewModel.UpdateAirport(selected.Id, cityBox.Text, nameBox.Text, codeBox.Text);
                LoadAirports();
            }
        }

        private void DeleteAirport_Click(object sender, RoutedEventArgs e)
        {
            if (AirportsSection.SelectedItem is not SimpleItemRow selected) return;

            _viewModel.DeleteAirport(selected.Id);
            LoadAirports();
        }

        private ContentDialog CreateDialog(string title, object content)
        {
            return new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };
        }
    }

    public class SimpleItemRow
    {
        public int Id { get; set; }
        public string MainValue { get; set; } = "";
        public string SecondaryValue { get; set; } = "";
        public string ThirdValue { get; set; } = "";
        public string DisplayText { get; set; } = "";
    }
}
