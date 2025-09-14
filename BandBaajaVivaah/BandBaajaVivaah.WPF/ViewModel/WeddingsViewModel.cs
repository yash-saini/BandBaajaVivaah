using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Windows;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class WeddingsViewModel : ViewModelBase
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;

        private ObservableCollection<WeddingDto> _weddings;
        public ObservableCollection<WeddingDto> Weddings
        {
            get => _weddings;
            set
            {
                _weddings = value;
                OnPropertyChanged(nameof(Weddings));
            }
        }

        private WeddingDto _selectedWedding;
        public WeddingDto SelectedWedding
        {
            get => _selectedWedding;
            set
            {
                _selectedWedding = value;
                OnPropertyChanged(nameof(SelectedWedding));
            }
        }

        public WeddingsViewModel(ApiClientService apiClient, NavigationService navigationService)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            Weddings = new ObservableCollection<WeddingDto>();
            LoadWeddingsAsync();
        }

        public async Task LoadWeddingsAsync()
        {
            var weddingsList = await _apiClient.GetWeddingsAsync();
            if (weddingsList != null)
            {
                Weddings.Clear();
                foreach (var wedding in weddingsList)
                {
                    Weddings.Add(wedding);
                }
            }
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public void AddWedding()
        {
            MessageBox.Show("Logic for adding a new wedding goes here.");
        }

        public async Task DeleteWedding()
        {
            if (SelectedWedding == null)
            {
                MessageBox.Show("Please select a wedding to delete.");
                return;
            }
            var result = MessageBox.Show($"Are you sure you want to delete the wedding - {SelectedWedding.WeddingName}?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var success = await _apiClient.DeleteWeddingAsync(SelectedWedding.WeddingID);
                if (success)
                {
                    Weddings.Remove(SelectedWedding);
                    SelectedWedding = null;
                }
                else
                {
                    MessageBox.Show("Failed to delete the wedding. Please try again.");
                }
            }
        }

        public bool CanEditOrDelete()
        {
            return SelectedWedding != null;
        }
    }
}
