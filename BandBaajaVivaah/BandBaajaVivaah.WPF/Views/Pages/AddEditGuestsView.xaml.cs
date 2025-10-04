using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel;
using BandBaajaVivaah.WPF.ViewModel.AddEditViewModel;
using System.Windows;
using System.Windows.Controls;

namespace BandBaajaVivaah.WPF.Views.Pages
{
    /// <summary>
    /// Interaction logic for AddEditGuestsView.xaml
    /// </summary>
    public partial class AddEditGuestsView : Page
    {
        AddEditGuestsViewModel? ViewModel => DataContext as AddEditGuestsViewModel;
        private readonly NavigationService _navigationService;
        private readonly ApiClientService _apiClient;
        private GuestsViewModel? _parentViewModel;
        private readonly int _weddingId;

        public AddEditGuestsView(ApiClientService apiClient, NavigationService navigationService, int weddingId ,GuestDto guest = null)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _navigationService = navigationService;
            _weddingId = weddingId;

            // Store a reference to the parent ViewModel if it's available
            if (navigationService.GetPreviousPage() is GuestsView guestsView &&
                guestsView.DataContext is GuestsViewModel guestsViewModel)
            {
                _parentViewModel = guestsViewModel;
            }

            // Create the view model with weddingId
            var viewModel = new AddEditGuestsViewModel(apiClient, navigationService, _weddingId, guest);

            // Hook up the refresh event
            viewModel.RefreshParentRequested += RefreshGuestsData;

            this.DataContext = viewModel;
        }

        private void Toolbar_BackButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel != null && ViewModel.HasErrors)
            {
                var result = MessageBox.Show("There are validation errors. Are you sure you want to discard your changes?",
                                             "Confirm Navigation",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    ViewModel.GoBack();
                }
            }
            else
            {
                ViewModel?.GoBack();
            }
        }

        private async void Toolbar_SaveButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.SaveAsync();
            }
        }

        private async Task RefreshGuestsData()
        {
            if (_parentViewModel != null)
            {
                await _parentViewModel.LoadDataAsync();
                return;
            }
            var guestsViewModel = new GuestsViewModel(_apiClient, _navigationService, _weddingId);
            await guestsViewModel.LoadDataAsync();
        }
    }
}
