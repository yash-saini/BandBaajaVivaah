using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.Grpc;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Windows;
using ExpenseUpdateService = BandBaajaVivaah.WPF.Services.ExpenseUpdateService;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class ExpensesViewModel : PageViewModel<ExpenseDto>
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly int _weddingId;
        public event EventHandler<string> ShowMessageRequested;
        private readonly ExpenseUpdateService _expenseUpdateService;
        private bool _isSubscribed = false;
        public event EventHandler<(string Message, string Title, Action<bool> Callback)> ShowConfirmationRequested;

        private ObservableCollection<ExpenseDto> _expenses;
        public ObservableCollection<ExpenseDto> Expenses
        {
            get => _expenses;
            set
            {
                _expenses = value;
                OnPropertyChanged(nameof(Expenses));
            }
        }

        private string _searchDescription;
        public string SearchDescription
        {
            get => _searchDescription;
            set
            {
                _searchDescription = value;
                OnPropertyChanged(nameof(SearchDescription));
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        private string _searchCategory;
        public string SearchCategory
        {
            get => _searchCategory;
            set
            {
                _searchCategory = value;
                OnPropertyChanged(nameof(SearchCategory));
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        private string _searchExpense;
        public string SearchExpense
        {
            get => _searchExpense;
            set
            {
                _searchExpense = value;
                OnPropertyChanged(nameof(SearchExpense));
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        public ExpensesViewModel(ApiClientService apiClient, NavigationService navigationService, int weddingId, ExpenseUpdateService expenseUpdateService = null)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _weddingId = weddingId;
            _expenseUpdateService = expenseUpdateService;

            if (_expenseUpdateService != null)
            {
                SubscribeToExpenseUpdates();
            }

            LoadDataAsync();
        }

        private async void SubscribeToExpenseUpdates()
        {
            if (_isSubscribed)
                return;

            try
            {
                _expenseUpdateService.OnExpenseUpdate += OnExpenseUpdateReceived;
                await _expenseUpdateService.SubscribeToWeddingUpdates(_weddingId);
                _isSubscribed = true;
            }
            catch (Exception ex)
            {
                ShowMessageRequested?.Invoke(this, $"Failed to subscribe to real-time updates: {ex.Message}");
            }
        }

        private void OnExpenseUpdateReceived(object sender, ExpenseUpdateEvent e)
        {
            // Use dispatcher to ensure UI updates happen on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                var expenseDto = new ExpenseDto
                {
                    ExpenseID = e.Expense.ExpenseId,
                    Description = e.Expense.Description,
                    Amount = (decimal)e.Expense.Amount,
                    Category = e.Expense.Category,
                };

                switch (e.Type)
                {
                    case ExpenseUpdateEvent.Types.UpdateType.Created:
                        HandleExpenseCreated(expenseDto);
                        break;
                    case ExpenseUpdateEvent.Types.UpdateType.Updated:
                        HandleExpenseUpdated(expenseDto);
                        break;
                    case ExpenseUpdateEvent.Types.UpdateType.Deleted:
                        HandleExpenseDeleted(expenseDto);
                        break;
                }
            });
        }

        private void HandleExpenseCreated(ExpenseDto expense)
        {
            if (AllItems.Any(g => g.ExpenseID == expense.ExpenseID))
                return;

            AllItems.Add(expense);
            UpdateDisplayedItems();
            ShowMessageRequested?.Invoke(this, $"New expense added: {expense.Amount}");
        }

        private void HandleExpenseUpdated(ExpenseDto expense)
        {
            var existing = AllItems.FirstOrDefault(g => g.ExpenseID == expense.ExpenseID);
            if (existing != null)
            {
                var index = AllItems.IndexOf(existing);
                AllItems[index] = expense;
                UpdateDisplayedItems();
            }
        }

        private void HandleExpenseDeleted(ExpenseDto expense)
        {
            var existing = AllItems.FirstOrDefault(g => g.ExpenseID == expense.ExpenseID);
            if (existing != null)
            {
                AllItems.Remove(existing);
                UpdateDisplayedItems();
            }
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public override async Task LoadDataAsync()
        {
            var expensesList = await _apiClient.GetExpensesAsync(_weddingId);
            if (expensesList != null)
            {
                AllItems = expensesList.ToList();
                UpdateDisplayedItems();
            }
        }

        protected override IEnumerable<ExpenseDto> ApplyFiltering(IEnumerable<ExpenseDto> items)
        {
            var filtered = items;
            if (!string.IsNullOrWhiteSpace(SearchDescription))
            {
                filtered = filtered.Where(e => e.Description != null && e.Description.IndexOf(SearchDescription, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (!string.IsNullOrWhiteSpace(SearchCategory))
            {
                filtered = filtered.Where(e => e.Category != null && e.Category.IndexOf(SearchCategory, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (!string.IsNullOrWhiteSpace(SearchExpense))
            {
                filtered = filtered.Where(w => w.Amount.ToString().StartsWith(SearchExpense));
            }
            return filtered;
        }

        public async Task DeleteExpense()
        {
            if (SelectedItem == null)
            {
                ShowMessageRequested?.Invoke(this, "Please select an expense to delete.");
                return;
            }

            ShowConfirmationRequested?.Invoke(this,
                ($"Are you sure you want to delete the expense '{SelectedItem.Description}'?",
                 "Confirm Delete",
                 async (confirmed) =>
                 {
                     if (confirmed)
                     {
                         var success = await _apiClient.DeleteExpenseAsync(SelectedItem.ExpenseID);
                         if (success)
                         {
                             await LoadDataAsync();
                             SelectedItem = null;
                         }
                         else
                         {
                             ShowMessageRequested?.Invoke(this, "Failed to delete the expense. Please try again.");
                         }
                     }
                 }
            ));
        }

        public void Dispose()
        {
            if (_expenseUpdateService != null && _isSubscribed)
            {
                _expenseUpdateService.OnExpenseUpdate -= OnExpenseUpdateReceived;
                _expenseUpdateService.Unsubscribe();
                _isSubscribed = false;
            }
        }
    }
}

