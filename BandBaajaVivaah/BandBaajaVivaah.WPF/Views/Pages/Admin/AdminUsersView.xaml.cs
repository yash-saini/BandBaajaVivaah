using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Admin;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NavigationService = BandBaajaVivaah.WPF.Services.NavigationService;

namespace BandBaajaVivaah.WPF.Views.Pages.Admin
{
    /// <summary>
    /// Interaction logic for AdminView.xaml
    /// </summary>
    public partial class AdminUsersView : Page
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        AdminUsersViewModel ViewModel => DataContext as AdminUsersViewModel;

        public AdminUsersView(ApiClientService apiClient, NavigationService navigationService)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _navigationService = navigationService;

            var viewModel = new AdminUsersViewModel(apiClient, navigationService);

            viewModel.ShowMessageRequested += (s, msg) => MessageBox.Show(msg);
            viewModel.ShowConfirmationRequested += (s, data) =>
            {
                var result = MessageBox.Show(data.Message, data.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                data.Callback(result == MessageBoxResult.Yes);
            };

            DataContext = viewModel;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel?.SelectedItem != null)
            {
                var editPage = new EditUserView(_apiClient, _navigationService, ViewModel.SelectedItem, ViewModel);
                _navigationService.NavigateTo(editPage);
            }
        }

        private void Toolbar_BackButtonClicked(object sender, System.EventArgs e) => ViewModel?.GoBack();
        private async void Toolbar_RefreshButtonClicked(object sender, System.EventArgs e) => await ViewModel?.LoadDataAsync();
        private void Toolbar_DeleteButtonClicked(object sender, System.EventArgs e)
        {
            if (ViewModel?.DeleteUserCommand.CanExecute(null) ?? false)
            {
                ViewModel.DeleteUserCommand.Execute(null);
            }
        }

        private void Toolbar_FirstPageClicked(object sender, System.EventArgs e) => ViewModel?.GoToFirstPage();
        private void Toolbar_PreviousPageClicked(object sender, System.EventArgs e) => ViewModel?.GoToPreviousPage();
        private void Toolbar_NextPageClicked(object sender, System.EventArgs e) => ViewModel?.GoToNextPage();
        private void Toolbar_LastPageClicked(object sender, System.EventArgs e) => ViewModel?.GoToLastPage();
    }
}

