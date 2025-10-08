using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;

namespace BandBaajaVivaah.WPF.ViewModel.AddEditViewModel
{
    public class AddEditTasksViewModel : ValidatableViewModelBase
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly TaskDto? _editingTask;
        private readonly int _weddingId;

        private bool _formSubmitAttempted = false;

        public bool ShowValidationSummary => _formSubmitAttempted && HasErrors;

        public string Title => _editingTask == null ? "Add New Task" : "Edit Task";

        private string _title = string.Empty;
        public string TitleText
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(TitleText));
                if (_formSubmitAttempted)
                    ValidateTitle();
            }
        }

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

        private DateTime? _dueDate;
        public DateTime? DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                OnPropertyChanged(nameof(DueDate));
                if (_formSubmitAttempted)
                    ValidateDate();
            }
        }
        private string _status = string.Empty;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public bool CanSave => !HasErrors;

        public bool WasSuccess { get; private set; } = false;

        public event Func<Task> RefreshParentRequested;

        public AddEditTasksViewModel(ApiClientService apiClient, NavigationService navigationService, int weddingId, TaskDto? task = null)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _editingTask = task;
            _weddingId = weddingId;

            if (_editingTask != null)
            {
                // EDIT MODE: Populate the form with the existing task data
                TitleText = _editingTask.Title ?? string.Empty;
                Description = _editingTask.Description ?? string.Empty;
                DueDate = _editingTask.DueDate;
                Status = _editingTask.Status ?? string.Empty;
            }
            else
            {
                // ADD MODE
                TitleText = string.Empty;
                Description = string.Empty;
                DueDate = DateTime.Now.AddDays(30);
                Status = "Not Started";
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
            ValidateTitle();
            ValidateDescription();
            ValidateDate();

            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(ShowValidationSummary));

            if (HasErrors)
            {
                return;
            }

            var dto = new CreateTaskDto
            {
                Title = TitleText,
                Description = Description,
                DueDate = DueDate,
                Status = Status,
                WeddingID = _weddingId
            };

            bool success;
            if (_editingTask == null) // Add Mode
            {
                var newTask = await _apiClient.CreateTaskAsync(dto);
                success = newTask != null;
            }
            else // Edit Mode
            {
                success = await _apiClient.UpdateTaskAsync(_editingTask.TaskID, dto);
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

        private void ValidateTitle()
        {
            ClearErrors(nameof(TitleText));
            if (string.IsNullOrWhiteSpace(TitleText))
            {
                AddError(nameof(TitleText), "Title cannot be empty.");
            }
            else if (TitleText.Length > 50)
            {
                AddError(nameof(TitleText), "Title cannot be longer than 50 characters.");
            }
        }

        private void ValidateDescription()
        {
            ClearErrors(nameof(Description));
            if (string.IsNullOrWhiteSpace(Description))
            {
                AddError(nameof(Description), "Description cannot be empty.");
            }
            else if (Description.Length > 200)
            {
                AddError(nameof(Description), "Description cannot be longer than 200 characters.");
            }
        }

        private void ValidateDate()
        {
            ClearErrors(nameof(DueDate));
            if (DueDate == null)
            {
                AddError(nameof(DueDate), "Due Date is required.");
            }
            else if (DueDate < DateTime.Today)
            {
                AddError(nameof(DueDate), "Due Date cannot be in the past.");
            }
        }
    }
}
