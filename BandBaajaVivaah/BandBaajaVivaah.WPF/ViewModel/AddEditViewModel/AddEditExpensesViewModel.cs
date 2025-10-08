using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;

namespace BandBaajaVivaah.WPF.ViewModel.AddEditViewModel
{
    public class AddEditExpensesViewModel : ValidatableViewModelBase
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly ExpenseDto? _editingExpense;
        private readonly int _weddingId;

        private bool _formSubmitAttempted = false;

        public bool ShowValidationSummary => _formSubmitAttempted && HasErrors;

        public string Title => _editingExpense == null ? "Add New Expense" : "Edit Expense";

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
                if (_formSubmitAttempted)
                    ValidateDescription();
            }
        }

        private decimal? _amount=0;
        private string _amountText = "0";
        public string AmountText
        {
            get => _amountText;
            set
            {
                _amountText = value;
                OnPropertyChanged(nameof(AmountText));

                // Try to parse the string. If it fails, the value is invalid (null).
                if (decimal.TryParse(value, out decimal parsedAmount))
                {
                    _amount = parsedAmount;
                }
                else
                {
                    _amount = null; // Invalid input like "abc" or empty string
                }

                if (_formSubmitAttempted) ValidateAmount();
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
                if (_formSubmitAttempted)
                    ValidateDate();
            }
        }

        public bool CanSave => !HasErrors;

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
                AmountText = _editingExpense.Amount.ToString();
                Category = _editingExpense.Category ?? string.Empty;
                PaymentDate = _editingExpense.PaymentDate;
            }
            else
            {
                // ADD MODE
                Description = string.Empty;
                AmountText = "0";
                Category = "Photography";
                PaymentDate = DateTime.Now;
            }
            ErrorsChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(ShowValidationSummary));
            };
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public async Task SaveAsync()
        {
            _formSubmitAttempted = true;
            ValidateDescription();
            ValidateAmount();
            ValidateDate();

            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(ShowValidationSummary));

            if (HasErrors)
            {
                return;
            }

            var dto = new CreateExpenseDto
            {
                Description = Description,
                Amount = _amount.HasValue ? _amount.Value : 0,
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

        private void ValidateDescription()
        {
            ClearErrors(nameof(Description));
            if (string.IsNullOrWhiteSpace(Description))
            {
                AddError(nameof(Description), "Description is required.");
            }
            else if (Description.Length > 200)
            {
                AddError(nameof(Description), "Description cannot exceed 200 characters.");
            }
        }

        private void ValidateAmount()
        {
            ClearErrors(nameof(AmountText)); // Validate the string property

            // Use the private decimal field for validation
            if (!_amount.HasValue)
            {
                AddError(nameof(AmountText), "Amount is required and must be a valid number.");
            }
            else if (_amount < 0)
            {
                AddError(nameof(AmountText), "Amount cannot be negative.");
            }
        }

        private void ValidateDate()
        {
            ClearErrors(nameof(PaymentDate));
            if (!PaymentDate.HasValue)
            {
                AddError(nameof(PaymentDate), "Payment Date is required.");
            }
            else if (PaymentDate > DateTime.Now)
            {
                AddError(nameof(PaymentDate), "Payment Date cannot be in the future.");
            }
        }
    }
}