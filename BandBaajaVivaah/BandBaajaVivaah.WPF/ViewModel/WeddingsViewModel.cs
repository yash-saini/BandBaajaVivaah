using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Windows;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class WeddingsViewModel : PageViewModel<WeddingDto>
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

        private string _searchName;
        public string SearchName
        {
            get => _searchName;
            set
            {
                _searchName = value;
                OnPropertyChanged(nameof(SearchName));
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        private string _searchBudget;
        public string SearchBudget
        {
            get => _searchBudget;
            set
            {
                _searchBudget = value;
                OnPropertyChanged(nameof(SearchBudget));
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        public WeddingsViewModel(ApiClientService apiClient, NavigationService navigationService)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            LoadDataAsync();
        }

        public override async Task LoadDataAsync()
        {
            var weddingsList = await _apiClient.GetWeddingsAsync();
            if (weddingsList != null)
            {
                AllItems = weddingsList.ToList(); // Setting the base class's master list
                CurrentPage = 1;
                UpdateDisplayedItems(); // Refreshing the view
            }
        }

        protected override IEnumerable<WeddingDto> ApplyFiltering(IEnumerable<WeddingDto> items)
        {
            IEnumerable<WeddingDto> filtered = items;

            // Apply Name Filter
            if (!string.IsNullOrWhiteSpace(SearchName))
            {
                filtered = filtered.Where(w => w.WeddingName.StartsWith(SearchName, StringComparison.OrdinalIgnoreCase));
            }

            // Apply Budget Filter
            if (!string.IsNullOrWhiteSpace(SearchBudget) && decimal.TryParse(SearchBudget, out var budget))
            {
                filtered = filtered.Where(w => w.TotalBudget >= budget);
            }

            return filtered;
        }


        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public async Task DeleteWedding()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Please select a wedding to delete.");
                return;
            }
            var result = MessageBox.Show($"Are you sure you want to delete the wedding - {SelectedItem.WeddingName}?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var success = await _apiClient.DeleteWeddingAsync(SelectedItem.WeddingID);
                if (success)
                {
                    await LoadDataAsync(); // Reload all data
                    SelectedItem = null;
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
