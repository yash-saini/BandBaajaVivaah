using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Windows;

namespace BandBaajaVivaah.WPF.ViewModel.AddEditViewModel
{
    public class AddEditTasksViewModel : ViewModelBase
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly TaskDto? _editingTask;
        private readonly int _weddingId;

        public string Title => _editingTask == null ? "Add New Task" : "Edit Task";

        private string _title = string.Empty;
        public string TitleText
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(TitleText));
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
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public async Task SaveAsync()
        {
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
    }
}
