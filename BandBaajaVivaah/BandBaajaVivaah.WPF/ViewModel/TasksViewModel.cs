using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.Grpc;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Windows;
using TaskUpdateService = BandBaajaVivaah.WPF.Services.TaskUpdateService;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class TasksViewModel : PageViewModel<TaskDto>
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly int _weddingId;
        private readonly TaskUpdateService _taskUpdateService;
        private bool _isSubscribed = false;

        public event EventHandler<string> ShowMessageRequested;
        public event EventHandler<(string Message, string Title, Action<bool> Callback)> ShowConfirmationRequested;

        private ObservableCollection<TaskDto> _tasks;
        public ObservableCollection<TaskDto> Tasks
        {
            get => _tasks;
            set
            {
                _tasks = value;
                OnPropertyChanged(nameof(Tasks));
            }
        }

        private string _searchTitle;
        public string SearchTitle
        {
            get => _searchTitle;
            set
            {
                _searchTitle = value;
                OnPropertyChanged(nameof(SearchTitle));
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
                CurrentPage = 1;
                UpdateDisplayedItems();
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
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        public TasksViewModel(ApiClientService apiClient, NavigationService navigationService, int weddingId, TaskUpdateService taskUpdateService=null)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _weddingId = weddingId;
            _taskUpdateService = taskUpdateService;
            if (_taskUpdateService != null)
            {
                SubscribeToTaskUpdates();
            }
            LoadDataAsync();
        }

        private async void SubscribeToTaskUpdates()
        {
            if (_isSubscribed)
                return;

            try
            {
                _taskUpdateService.OnTasksUpdate += OnTaskUpdateReceived;
                await _taskUpdateService.SubscribeToWeddingUpdates(_weddingId);
                _isSubscribed = true;
            }
            catch (Exception ex)
            {
                ShowMessageRequested?.Invoke(this, $"Failed to subscribe to real-time updates: {ex.Message}");
            }
        }

        private void OnTaskUpdateReceived(object sender, TaskUpdateEvent e)
        {
            if (e.Task.WeddingId != _weddingId)
                return;

            // Use dispatcher to ensure UI updates happen on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                var taskDto = new TaskDto
                {
                    TaskID = e.Task.TaskId,
                    Title = e.Task.Title,
                    Description = e.Task.Description,
                    Status = e.Task.Status,
                    DueDate = e.Task.DueDate?.ToDateTime()
                };

                switch (e.Type)
                {
                    case TaskUpdateEvent.Types.UpdateType.Created:
                        HandleTaskCreated(taskDto);
                        break;
                    case TaskUpdateEvent.Types.UpdateType.Updated:
                        HandleTaskUpdated(taskDto);
                        break;
                    case TaskUpdateEvent.Types.UpdateType.Deleted:
                        HandleTaskDeleted(taskDto);
                        break;
                }
            });
        }

        private void HandleTaskCreated(TaskDto tasks)
        {
            if (AllItems.Any(g => g.TaskID == tasks.TaskID))
                return;

            AllItems.Add(tasks);
            UpdateDisplayedItems();
            ShowMessageRequested?.Invoke(this, $"New task added: {tasks.Title}");
        }

        private void HandleTaskUpdated(TaskDto tasks)
        {
            var existingTask = AllItems.FirstOrDefault(g => g.TaskID == tasks.TaskID);
            if (existingTask != null)
            {
                var index = AllItems.IndexOf(existingTask);
                AllItems[index] = tasks;
                UpdateDisplayedItems();
            }
        }

        private void HandleTaskDeleted(TaskDto tasks)
        {
            var existingTask = AllItems.FirstOrDefault(g => g.TaskID == tasks.TaskID);
            if (existingTask != null)
            {
                AllItems.Remove(existingTask);
                UpdateDisplayedItems();
            }
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public override async Task LoadDataAsync()
        {
            var tasksList = await _apiClient.GetTasksAsync(_weddingId);
            if (tasksList != null)
            {
                AllItems = tasksList.ToList();
                UpdateDisplayedItems();
            }
        }

        protected override IEnumerable<TaskDto> ApplyFiltering(IEnumerable<TaskDto> items)
        {
            if (!string.IsNullOrWhiteSpace(SearchTitle))
            {
                items = items.Where(t => t.Title != null && t.Title.StartsWith(SearchTitle, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(Status))
            {
                items = items.Where(t => t.Status != null && t.Status.StartsWith(Status, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(Description))
            {
                items = items.Where(t => t.Description != null && t.Description.StartsWith(Description, StringComparison.OrdinalIgnoreCase));
            }
            return items;
        }

        public async Task DeleteTasks()
        {
            if (SelectedItem == null)
            {
                ShowMessageRequested?.Invoke(this, "Please select a task to delete.");
                return;
            }

            ShowConfirmationRequested?.Invoke(this,
                ($"Are you sure you want to delete the task '{SelectedItem.Title}'?",
                 "Confirm Delete",
                 async (confirmed) =>
                 {
                     if (confirmed)
                     {
                         var success = await _apiClient.DeleteTaskAsync(SelectedItem.TaskID);
                         if (success)
                         {
                             await LoadDataAsync(); // Reload all data
                             SelectedItem = null;
                         }
                         else
                         {
                             ShowMessageRequested?.Invoke(this, "Failed to delete the task. Please try again.");
                         }
                     }
                 }
            ));
        }

        public void Dispose()
        {
            if (_taskUpdateService != null && _isSubscribed)
            {
                _taskUpdateService.OnTasksUpdate -= OnTaskUpdateReceived;
                _taskUpdateService.Unsubscribe();
                _isSubscribed = false;
            }
        }
    }
}
