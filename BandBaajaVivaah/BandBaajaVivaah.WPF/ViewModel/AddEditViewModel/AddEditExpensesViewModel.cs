using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Windows;

namespace BandBaajaVivaah.WPF.ViewModel.AddEditViewModel
{
    public class AddEditExpensesViewModel : ViewModelBase
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly ExpenseDto? _editingExpense;
        private readonly int _weddingId;

        public string Title => _editingExpense == null ? "Add New Expense" : "Edit Expense";

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private decimal? _amount=0;
        public decimal? Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        private string _category = string.Empty;
        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        private DateTime? _paymentDate = DateTime.Now;
        public DateTime? PaymentDate
        {
            get => _paymentDate;
            set
            {
                _paymentDate = value;
                OnPropertyChanged(nameof(PaymentDate));
            }
        }

        public bool WasSuccess { get; private set; } = false;

        public event Func<Task> RefreshParentRequested;

        public AddEditExpensesViewModel(ApiClientService apiClient, NavigationService navigationService, int weddingId, ExpenseDto? expense = null)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _editingExpense = expense;
            _weddingId = weddingId;

            if (_editingExpense != null)
            {
                // EDIT MODE: Populate the form with the existing expense data
                Description = _editingExpense.Description ?? string.Empty;
                Amount = _editingExpense.Amount;
                Category = _editingExpense.Category ?? string.Empty;
                PaymentDate = _editingExpense.PaymentDate;
            }
            else
            {
                // ADD MODE
                Description = string.Empty;
                Amount = 0;
                Category = "Photography";
                PaymentDate = DateTime.Now;
            }
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public async Task SaveAsync()
        {
            var dto = new CreateExpenseDto
            {
                Description = Description,
                Amount = Amount.HasValue ? Amount.Value : 0,
                Category = Category,
                PaymentDate = PaymentDate ?? DateTime.Now,
                WeddingID = _weddingId
            };

            bool success;
            if (_editingExpense == null) // Add Mode
            {
                var newExpense = await _apiClient.CreateExpenseAsync(dto);
                success = newExpense != null;
            }
            else // Edit Mode
            {
                success = await _apiClient.UpdateExpenseAsync(_editingExpense.ExpenseID, dto);
            }

            if (success)
            {
                WasSuccess = true;
                await (RefreshParentRequested?.Invoke() ?? Task.CompletedTask);
                _navigationService.GoBack();
            }
            else
            {
                await Task.CompletedTask;
            }
        }
    }
}