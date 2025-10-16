using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.Grpc;
using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using BandBaajaVivaah.WPF.Views.Pages;
using System.Windows;
using System.Windows.Input;
using ExpenseUpdateService = BandBaajaVivaah.WPF.Services.ExpenseUpdateService;
using GuestUpdateService = BandBaajaVivaah.WPF.Services.GuestUpdateService;
using TaskUpdateService = BandBaajaVivaah.WPF.Services.TaskUpdateService;
using WeddingUpdateService = BandBaajaVivaah.WPF.Services.WeddingUpdateService;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class WeddingsViewModel : PageViewModel<WeddingDto>, IDisposable
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly UserDto _targetUser;
        private readonly bool _isAdminMode;
        private readonly GuestUpdateService _guestUpdateService;
        private readonly ExpenseUpdateService _expenseUpdateService;
        private readonly TaskUpdateService _taskUpdateService;
        private readonly WeddingUpdateService _weddingUpdateService;
        private bool _isSubscribed = false;
        private int _currentUserId;

        public bool IsAdminMode => _isAdminMode;
        public UserDto? TargetUser => _isAdminMode ? _targetUser : null;

        public ICommand ManageGuestsCommand { get; }
        public ICommand ManageTasksCommand { get; }
        public ICommand ManageExpensesCommand { get; }
        public ICommand AddWeddingCommand { get; }

        public event EventHandler<string> ShowMessageRequested;
        public event EventHandler<(string Message, string Title, Action<bool> Callback)> ShowConfirmationRequested;

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

        // Regular user constructor
        public WeddingsViewModel(ApiClientService apiClient, NavigationService navigationService)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _guestUpdateService = new GuestUpdateService("https://localhost:7159");
            _expenseUpdateService = new ExpenseUpdateService("https://localhost:7159");
            _taskUpdateService = new TaskUpdateService("https://localhost:7159");
            _weddingUpdateService = new WeddingUpdateService("https://localhost:7159");

            ManageGuestsCommand = new RelayCommand(NavigateToGuests);
            ManageTasksCommand = new RelayCommand(NavigateToTasks);
            ManageExpensesCommand = new RelayCommand(NavigateToExpenses);
            AddWeddingCommand = new RelayCommand(_ => NavigateToAddWedding());

            // Initialize asynchronously to get user ID from first wedding
            _ = InitializeAsync();
        }

        // Admin user constructor
        public WeddingsViewModel(ApiClientService apiClient, NavigationService navigationService, UserDto targetUser)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _targetUser = targetUser;
            _isAdminMode = true;
            _currentUserId = targetUser.UserID;
            _guestUpdateService = new GuestUpdateService("https://localhost:7159");
            _expenseUpdateService = new ExpenseUpdateService("https://localhost:7159");
            _taskUpdateService = new TaskUpdateService("https://localhost:7159");
            _weddingUpdateService = new WeddingUpdateService("https://localhost:7159");

            ManageGuestsCommand = new RelayCommand(NavigateToGuests);
            ManageTasksCommand = new RelayCommand(NavigateToTasks);
            ManageExpensesCommand = new RelayCommand(NavigateToExpenses);
            AddWeddingCommand = new RelayCommand(_ => NavigateToAddWedding());

            SubscribeToWeddingUpdates();
            LoadDataAsync();
        }

        // Initialize for regular users
        private async Task InitializeAsync()
        {
            await LoadDataAsync();

            // Get current user ID from the first wedding's owner
            if (AllItems != null && AllItems.Any())
            {
                _currentUserId = AllItems.First().OwnerUserId;
                Console.WriteLine($"WeddingsViewModel: Regular user ID determined as {_currentUserId}");
                SubscribeToWeddingUpdates();
            }
            else
            {
                Console.WriteLine("WeddingsViewModel: No weddings found for current user, cannot subscribe yet");
            }
        }

        private async void SubscribeToWeddingUpdates()
        {
            if (_isSubscribed || _weddingUpdateService == null || _currentUserId == 0)
            {
                Console.WriteLine($"WeddingsViewModel: Cannot subscribe - isSubscribed={_isSubscribed}, userId={_currentUserId}");
                return;
            }

            try
            {
                _weddingUpdateService.OnWeddingUpdate += OnWeddingUpdateReceived;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _weddingUpdateService.SubscribeToUserWeddingUpdates(_currentUserId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"WeddingsViewModel: Background subscription error: {ex.Message}");
                    }
                });

                _isSubscribed = true;
                Console.WriteLine($"WeddingsViewModel: Successfully started subscription for user {_currentUserId}, isAdmin={_isAdminMode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WeddingsViewModel: Failed to subscribe to wedding updates: {ex.Message}");
                ShowMessageRequested?.Invoke(this, $"Failed to subscribe to wedding updates: {ex.Message}");
            }
        }

        private void OnWeddingUpdateReceived(object sender, WeddingUpdateEvent e)
        {
            try
            {
                Console.WriteLine($"WeddingsViewModel: Received {e.Type} event for wedding {e.Wedding.WeddingId}, owner={e.Wedding.OwnerUserId}");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var weddingDto = new WeddingDto
                    {
                        WeddingID = e.Wedding.WeddingId,
                        WeddingName = e.Wedding.WeddingName,
                        WeddingDate = e.Wedding.WeddingDate.ToDateTime(),
                        TotalBudget = (decimal)e.Wedding.TotalBudget,
                        OwnerUserId = e.Wedding.OwnerUserId
                    };

                    Console.WriteLine($"WeddingsViewModel: Processing {e.Type} for wedding {weddingDto.WeddingID}");

                    switch (e.Type)
                    {
                        case WeddingUpdateEvent.Types.UpdateType.Created:
                            HandleWeddingCreated(weddingDto);
                            break;
                        case WeddingUpdateEvent.Types.UpdateType.Updated:
                            HandleWeddingUpdated(weddingDto);
                            break;
                        case WeddingUpdateEvent.Types.UpdateType.Deleted:
                            HandleWeddingDeleted(weddingDto);
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WeddingsViewModel: Error processing wedding update: {ex.Message}");
            }
        }

        private void HandleWeddingCreated(WeddingDto wedding)
        {
            if (AllItems.Any(w => w.WeddingID == wedding.WeddingID))
            {
                Console.WriteLine($"WeddingsViewModel: Wedding {wedding.WeddingID} already exists, skipping");
                return;
            }

            AllItems.Add(wedding);
            UpdateDisplayedItems();
            ShowMessageRequested?.Invoke(this, $"New wedding added: {wedding.WeddingName}");
            Console.WriteLine($"WeddingsViewModel: Added wedding {wedding.WeddingID} ({wedding.WeddingName}) to list");
        }

        private void HandleWeddingUpdated(WeddingDto wedding)
        {
            var existingWedding = AllItems.FirstOrDefault(w => w.WeddingID == wedding.WeddingID);
            if (existingWedding != null)
            {
                var index = AllItems.IndexOf(existingWedding);
                AllItems[index] = wedding;
                UpdateDisplayedItems();
                Console.WriteLine($"WeddingsViewModel: Updated wedding {wedding.WeddingID} in list");
            }
            else
            {
                Console.WriteLine($"WeddingsViewModel: Wedding {wedding.WeddingID} not found for update");
            }
        }

        private void HandleWeddingDeleted(WeddingDto wedding)
        {
            var existingWedding = AllItems.FirstOrDefault(w => w.WeddingID == wedding.WeddingID);
            if (existingWedding != null)
            {
                AllItems.Remove(existingWedding);
                UpdateDisplayedItems();
                Console.WriteLine($"WeddingsViewModel: Removed wedding {wedding.WeddingID} from list");
            }
            else
            {
                Console.WriteLine($"WeddingsViewModel: Wedding {wedding.WeddingID} not found for deletion");
            }
        }

        private void NavigateToAddWedding()
        {
            if (_isAdminMode)
            {
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
            IEnumerable<WeddingDto> weddings;

            if (_isAdminMode)
            {
                weddings = await _apiClient.GetWeddingsForUserAsync(_targetUser.UserID);
                Console.WriteLine($"WeddingsViewModel: Loaded {weddings?.Count() ?? 0} weddings for user {_targetUser.UserID} (Admin mode)");
            }
            else
            {
                weddings = await _apiClient.GetWeddingsAsync();
                Console.WriteLine($"WeddingsViewModel: Loaded {weddings?.Count() ?? 0} weddings (User mode)");
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
                var taskPage = new TasksView(_apiClient, _navigationService, wedding.WeddingID, _taskUpdateService);
                _navigationService.NavigateTo(taskPage);
            }
        }

        private void NavigateToExpenses(object obj)
        {
            if (obj is WeddingDto wedding)
            {
                var expensePage = new ExpensesView(_apiClient, _navigationService, wedding.WeddingID, _expenseUpdateService);
                _navigationService.NavigateTo(expensePage);
            }
        }

        protected override IEnumerable<WeddingDto> ApplyFiltering(IEnumerable<WeddingDto> items)
        {
            IEnumerable<WeddingDto> filtered = items;

            if (!string.IsNullOrWhiteSpace(SearchName))
            {
                filtered = filtered.Where(w => w.WeddingName.StartsWith(SearchName, StringComparison.OrdinalIgnoreCase));
            }

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
                 async void (confirmed) =>
                 {
                     if (confirmed)
                     {
                         try
                         {
                             bool success;

                             if (_isAdminMode)
                             {
                                 success = await _apiClient.DeleteWeddingByAdminAsync(weddingToDelete.WeddingID);
                                 Console.WriteLine($"WeddingsViewModel: Admin deleted wedding {weddingToDelete.WeddingID}, success={success}");
                             }
                             else
                             {
                                 success = await _apiClient.DeleteWeddingAsync(weddingToDelete.WeddingID);
                                 Console.WriteLine($"WeddingsViewModel: User deleted wedding {weddingToDelete.WeddingID}, success={success}");
                             }

                             if (success)
                             {
                                 // Don't call LoadDataAsync - real-time update will handle it
                                 AllItems.Remove(weddingToDelete);
                                 UpdateDisplayedItems();
                                 SelectedItem = null;
                                 ShowMessageRequested?.Invoke(this, "Wedding deleted successfully.");
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

        public void Dispose()
        {
            if (_weddingUpdateService != null && _isSubscribed)
            {
                _weddingUpdateService.OnWeddingUpdate -= OnWeddingUpdateReceived;
                // Don't call Unsubscribe() as it cancels ALL subscriptions
                _expenseUpdateService.Unsubscribe();
                _isSubscribed = false;
                Console.WriteLine($"WeddingsViewModel: Disposed, unsubscribed from user {_currentUserId} wedding updates");
            }
        }
    }
}