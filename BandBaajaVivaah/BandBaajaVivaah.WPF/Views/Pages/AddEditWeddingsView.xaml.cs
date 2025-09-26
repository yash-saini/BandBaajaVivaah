using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel;
using BandBaajaVivaah.WPF.ViewModel.AddEditViewModel;
using System.Windows.Controls;

namespace BandBaajaVivaah.WPF.Views.Pages
{
    /// <summary>
    /// Interaction logic for AddEditWeddingsView.xaml
    /// </summary>
    public partial class AddEditWeddingsView : Page
    {
        AddEditWeddingViewModel? ViewModel => DataContext as AddEditWeddingViewModel;
        private readonly NavigationService _navigationService;
        private readonly ApiClientService _apiClient;
        private WeddingsViewModel? _parentViewModel;

        public AddEditWeddingsView(ApiClientService apiClient, NavigationService navigationService, WeddingDto wedding = null)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _navigationService = navigationService;
            if (navigationService.GetPreviousPage() is WeddingsView weddingsView &&
                 weddingsView.DataContext is WeddingsViewModel weddingsViewModel)
            {
                _parentViewModel = weddingsViewModel;
            }

            var viewModel = new AddEditWeddingViewModel(apiClient, navigationService, wedding);

            // Hook up the refresh event
            viewModel.RefreshParentRequested += RefreshWeddingsData;

            this.DataContext = viewModel;
        }

        private void Toolbar_BackButtonClicked(object sender, EventArgs e)
        {
            ViewModel?.GoBack();
        }

        private async void Toolbar_SaveButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.SaveAsync();
            }
        }

        private async Task RefreshWeddingsData()
        {
            if (_parentViewModel != null)
            {
                await _parentViewModel.LoadDataAsync();
                return;
            }
            var weddingsViewModel = new WeddingsViewModel(_apiClient, _navigationService);
            await weddingsViewModel.LoadDataAsync();
        }
    }
}
