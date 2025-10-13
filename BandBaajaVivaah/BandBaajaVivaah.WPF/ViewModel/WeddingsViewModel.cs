using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using BandBaajaVivaah.WPF.Views.Pages;
using BandBaajaVivaah.WPF.Views.Pages.Admin;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class WeddingsViewModel : PageViewModel<WeddingDto>
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly UserDto _targetUser;
        private readonly bool _isAdminMode;
        private readonly GuestUpdateService _guestUpdateService;
        public bool IsAdminMode => _isAdminMode;
        public UserDto? TargetUser => _isAdminMode ? _targetUser : null;

        public ICommand ManageGuestsCommand { get; }
        public ICommand ManageTasksCommand { get; }
        public ICommand ManageExpensesCommand { get; }
        public ICommand AddWeddingCommand { get; }

        public event EventHandler<string> ShowMessageRequested;
        public event EventHandler<(string Message, string Title, Action<bool> Callback)> ShowConfirmationRequested;

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
            _guestUpdateService = new GuestUpdateService("https://localhost:7159");

            ManageGuestsCommand = new RelayCommand(NavigateToGuests);
            ManageTasksCommand = new RelayCommand(NavigateToTasks);
            ManageExpensesCommand = new RelayCommand(NavigateToExpenses);
            AddWeddingCommand = new RelayCommand(_ => NavigateToAddWedding());

            LoadDataAsync();
        }

        public WeddingsViewModel(ApiClientService apiClient, NavigationService navigationService, UserDto targetUser)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _targetUser = targetUser;
            _isAdminMode = true;

            ManageGuestsCommand = new RelayCommand(NavigateToGuests);
            ManageTasksCommand = new RelayCommand(NavigateToTasks);
            ManageExpensesCommand = new RelayCommand(NavigateToExpenses);
            AddWeddingCommand = new RelayCommand(_ => NavigateToAddWedding());

            LoadDataAsync();
        }

        private void NavigateToAddWedding()
        {
            if (_isAdminMode)
            {
                // Use existing AddEditWeddingsView but pass the target user ID
                var addEditView = new AddEditWeddingsView(_apiClient, _navigationService, null, _targetUser.UserID);
                _navigationService.NavigateTo(addEditView);
            }
            else
            {
                var addEditView = new AddEditWeddingsView(_apiClient, _navigationService);
                _navigationService.NavigateTo(addEditView);
            }
        }

        public override async Task LoadDataAsync()
        {
            //var weddingsList = await _apiClient.GetWeddingsAsync();
            //if (weddingsList != null)
            //{
            //    AllItems = weddingsList.ToList(); // Setting the base class's master list
            //    CurrentPage = 1;
            //    UpdateDisplayedItems(); // Refreshing the view
            //}
            IEnumerable<WeddingDto> weddings;

            if (_isAdminMode)
            {
                weddings = await _apiClient.GetWeddingsForUserAsync(_targetUser.UserID);
            }
            else
            {
                weddings = await _apiClient.GetWeddingsAsync();
            }

            if (weddings != null)
            {
                AllItems = weddings.ToList();
                UpdateDisplayedItems();
            }
            else
            {
                ShowMessageRequested?.Invoke(this, "Failed to load weddings.");
                AllItems = new List<WeddingDto>();
                UpdateDisplayedItems();
            }
        }

        private void NavigateToGuests(object obj)
        {
            if (obj is WeddingDto wedding)
            {
                var guestPage = new GuestsView(_apiClient, _navigationService, wedding.WeddingID, _guestUpdateService);
                _navigationService.NavigateTo(guestPage);
            }
        }

        private void NavigateToTasks(object obj)
        {
            if (obj is WeddingDto wedding)
            {
                var taskPage = new TasksView(_apiClient, _navigationService, wedding.WeddingID);
                _navigationService.NavigateTo(taskPage);
            }
        }

        private void NavigateToExpenses(object obj)
        {
            if (obj is WeddingDto wedding)
            {
                var expensePage = new ExpensesView(_apiClient, _navigationService, wedding.WeddingID);
                _navigationService.NavigateTo(expensePage);
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
                ShowMessageRequested?.Invoke(this, "Please select a wedding to delete.");
                return;
            }

            var weddingToDelete = SelectedItem;

            ShowConfirmationRequested?.Invoke(this,
                ($"Are you sure you want to delete the wedding - {weddingToDelete.WeddingName}?",
                 "Confirm Delete",
                 // Use async void instead of async (which returns Task)
                 async void (confirmed) =>
                 {
                     if (confirmed)
                     {
                         try
                         {
                             bool success;

                             // Use appropriate delete method based on mode
                             if (_isAdminMode)
                             {
                                 success = await _apiClient.DeleteWeddingByAdminAsync(weddingToDelete.WeddingID);
                             }
                             else
                             {
                                 success = await _apiClient.DeleteWeddingAsync(weddingToDelete.WeddingID);
                             }

                             if (success)
                             {
                                 ShowMessageRequested?.Invoke(this, "Wedding deleted successfully.");
                                 await LoadDataAsync(); // Reload all data
                                 SelectedItem = null;
                             }
                             else
                             {
                                 ShowMessageRequested?.Invoke(this,
                                     _isAdminMode
                                         ? "Failed to delete the wedding. The server returned an error."
                                         : "Failed to delete the wedding. You may not have permission.");
                             }
                         }
                         catch (Exception ex)
                         {
                             ShowMessageRequested?.Invoke(this, $"An error occurred: {ex.Message}");
                         }
                     }
                 }
            ));
        }
    }
}
