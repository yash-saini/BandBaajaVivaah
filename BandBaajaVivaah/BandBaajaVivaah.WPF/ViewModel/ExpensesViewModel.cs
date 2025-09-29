using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Collections.ObjectModel;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class ExpensesViewModel : PageViewModel<ExpenseDto>
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly int _weddingId;
        public event EventHandler<string> ShowMessageRequested;
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

        public ExpensesViewModel(ApiClientService apiClient, NavigationService navigationService, int weddingId)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _weddingId = weddingId;
            LoadDataAsync();
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
                Expenses = new ObservableCollection<ExpenseDto>(AllItems);
                CurrentPage = 1;
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
    }
}

