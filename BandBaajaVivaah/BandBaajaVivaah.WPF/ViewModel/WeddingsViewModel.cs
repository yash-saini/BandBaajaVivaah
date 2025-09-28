using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using BandBaajaVivaah.WPF.Views.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class WeddingsViewModel : PageViewModel<WeddingDto>
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        public ICommand ManageGuestsCommand { get; }
        public ICommand ManageTasksCommand { get; }
        public ICommand ManageExpensesCommand { get; }

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
            ManageGuestsCommand = new RelayCommand(NavigateToGuests);
            ManageTasksCommand = new RelayCommand(NavigateToTasks);
            ManageExpensesCommand = new RelayCommand(NavigateToExpenses);

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

        private void NavigateToGuests(object obj)
        {
            if (obj is WeddingDto wedding)
            {
                var guestPage = new GuestsView(_apiClient, _navigationService, wedding.WeddingID);
                _navigationService.NavigateTo(guestPage);
            }
        }

        private void NavigateToTasks(object obj)
        {
            if (obj is WeddingDto wedding)
            {
                MessageBox.Show("Navigate to Tasks Page for wedding: " + wedding.WeddingName);
            }
        }

        private void NavigateToExpenses(object obj)
        {
            if (obj is WeddingDto wedding)
            {
                MessageBox.Show("Navigate to Expenses Page for wedding: " + wedding.WeddingName);
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
            if (!string.IsNullOrWhiteSpace(SearchBudget))
            {
                filtered = filtered.Where(w => w.TotalBudget.ToString().StartsWith(SearchBudget));
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
    }
}
