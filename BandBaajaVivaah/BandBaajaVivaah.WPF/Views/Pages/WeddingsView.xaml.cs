using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using NavigationService = BandBaajaVivaah.WPF.Services.NavigationService;

namespace BandBaajaVivaah.WPF.Views.Pages
{
    /// <summary>
    /// Interaction logic for WeddingsView.xaml
    /// </summary>
    public partial class WeddingsView : Page
    {
        WeddingsViewModel? ViewModel => DataContext as WeddingsViewModel;
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;

        public WeddingsView(ApiClientService apiClient, NavigationService navigationService)
        {
            InitializeComponent();
            this.DataContext = new WeddingsViewModel(apiClient, navigationService);
            _apiClient = apiClient;
            _navigationService = navigationService;
        }

        private void Toolbar_BackButtonClicked(object sender, EventArgs e)
        {
            ViewModel?.GoBack();
        }

        private void Toolbar_AddButtonClicked(object sender, EventArgs e)
        {
            var formPage = new AddEditWeddingsView(_apiClient, _navigationService);
            _navigationService.NavigateTo(formPage);
        }

        private async void Toolbar_DeleteButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.DeleteWedding();
            }
        }

        private async void PageToolBarView_RefreshButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadWeddingsAsync();
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ViewModel?.SelectedWedding != null)
            {
                var formPage = new AddEditWeddingsView(_apiClient, _navigationService, ViewModel.SelectedWedding);
                _navigationService.NavigateTo(formPage);
            }
        }
    }
}
