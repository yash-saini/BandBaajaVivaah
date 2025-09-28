using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class TasksViewModel : PageViewModel<TaskDto>
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly int _weddingId;

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

        public TasksViewModel(ApiClientService apiClient, NavigationService navigationService, int weddingId)
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
            var tasksList = await _apiClient.GetTasksAsync(_weddingId);
            if (tasksList != null)
            {
                AllItems = tasksList.ToList();
                Tasks = new ObservableCollection<TaskDto>(AllItems);
                CurrentPage = 1;
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
                MessageBox.Show("Please select a task to delete.");
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete the task '{SelectedItem.Title}'?",
                                       "Confirm Delete", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _apiClient.DeleteTaskAsync(SelectedItem.TaskID);
                if (success)
                {
                    await LoadDataAsync(); // Reload all data
                    SelectedItem = null;
                }
                else
                {
                    MessageBox.Show("Failed to delete the task. Please try again.");
                }
            }
        }
    }
}
