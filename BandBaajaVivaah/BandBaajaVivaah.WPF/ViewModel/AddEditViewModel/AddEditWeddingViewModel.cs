using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;

namespace BandBaajaVivaah.WPF.ViewModel.AddEditViewModel
{
    public class AddEditWeddingViewModel : ValidatableViewModelBase
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly WeddingDto? _editingWedding;

        private bool _formSubmitAttempted = false;

        public bool ShowValidationSummary => _formSubmitAttempted && HasErrors;

        public string Title => _editingWedding == null ? "Add New Wedding" : "Edit Wedding";

        private string _weddingName = string.Empty;
        public string WeddingName
        {
            get => _weddingName;
            set
            {
                _weddingName = value;
                OnPropertyChanged(nameof(WeddingName));
                if (_formSubmitAttempted)
                    ValidateWeddingName();
            }
        }

        private DateTime? _weddingDate = DateTime.Now;
        public DateTime? WeddingDate
        {
            get => _weddingDate;
            set
            {
                _weddingDate = value;
                OnPropertyChanged(nameof(WeddingDate));
                if (_formSubmitAttempted)
                    ValidateDate();
            }
        }

        private decimal? _totalBudget = 0;
        private string _budgetText = "0";
        public string BudgetText
        {
            get => _budgetText;
            set
            {
                _budgetText = value;
                OnPropertyChanged(nameof(BudgetText));

                // Try to parse the string. If it fails, the value is invalid (null).
                if (decimal.TryParse(value, out decimal parsedAmount))
                {
                    _totalBudget = parsedAmount;
                }
                else
                {
                    _totalBudget = null; // Invalid input like "abc" or empty string
                }

                if (_formSubmitAttempted) ValidateBudget();
            }
        }

        public bool CanSave => !HasErrors;

        public bool WasSuccess { get; private set; } = false;

        public event Func<Task> RefreshParentRequested;

        public AddEditWeddingViewModel(ApiClientService apiClient, NavigationService navigationService, WeddingDto? wedding = null)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _editingWedding = wedding;

            if (_editingWedding != null)
            {
                // EDIT MODE: Populate the form with the existing wedding's data
                WeddingName = _editingWedding.WeddingName;
                WeddingDate = _editingWedding.WeddingDate;
                BudgetText = _editingWedding.TotalBudget.ToString();
            }
            else
            {
                // ADD MODE
                WeddingDate = DateTime.Now;
                BudgetText = "0";
                WeddingName = string.Empty;
            }
            ErrorsChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(ShowValidationSummary));
            };
        }

        public async Task SaveAsync()
        {
            _formSubmitAttempted = true;
            ValidateWeddingName();
            ValidateDate();
            ValidateBudget();

            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(ShowValidationSummary));

            if (HasErrors)
            {
                return;
            }
            var dto = new CreateWeddingDto
            {
                WeddingName = this.WeddingName,
                WeddingDate = this.WeddingDate.HasValue ? this.WeddingDate.Value : DateTime.Today,
                TotalBudget = this._totalBudget.HasValue ? this._totalBudget.Value : 0
            };

            bool success;
            if (_editingWedding == null) // Add Mode
            {
                var newWedding = await _apiClient.CreateWeddingAsync(dto);
                success = newWedding != null;
            }
            else // Edit Mode
            {
                success = await _apiClient.UpdateWeddingAsync(_editingWedding.WeddingID, dto);
            }

            if (success)
            {
                WasSuccess = true;
                RefreshParentRequested?.Invoke();
                _navigationService.GoBack();
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        private void ValidateWeddingName()
        {
            ClearErrors(nameof(WeddingName));
            if (string.IsNullOrWhiteSpace(WeddingName))
            {
                AddError(nameof(WeddingName), "Wedding Name is required.");
            }
            else if (WeddingName.Length > 100)
            {
                AddError(nameof(WeddingName), "Wedding Name cannot exceed 100 characters.");
            }
        }

        private void ValidateDate()
        {
            ClearErrors(nameof(WeddingDate));
            if (!WeddingDate.HasValue)
            {
                AddError(nameof(WeddingDate), "Wedding Date is required.");
            }
            else if (WeddingDate.Value.Date < DateTime.Now.Date)
            {
                AddError(nameof(WeddingDate), "Wedding Date cannot be in the past.");
            }
        }

        private void ValidateBudget()
        {
            ClearErrors(nameof(BudgetText));
            if (!_totalBudget.HasValue)
            {
                AddError(nameof(BudgetText), "Total Budget is required.");
            }
            else if (_totalBudget < 0)
            {
                AddError(nameof(BudgetText), "Total Budget cannot be negative.");
            }
        }
    }
}
