using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NavigationService = BandBaajaVivaah.WPF.Services.NavigationService;

namespace BandBaajaVivaah.WPF.Views.Pages
{
    /// <summary>
    /// Interaction logic for TasksView.xaml
    /// </summary>
    public partial class TasksView : Page
    {
        TasksViewModel? ViewModel => DataContext as TasksViewModel;
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly TaskUpdateService _taskUpdateService;
        private readonly int _weddingId;

        public TasksView(ApiClientService apiClient, NavigationService navigationService, int weddingId, TaskUpdateService taskUpdateService)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _weddingId = weddingId;
            _navigationService = navigationService;
            _taskUpdateService = taskUpdateService;
            var viewModel = new TasksViewModel(_apiClient, _navigationService, weddingId, _taskUpdateService);
            viewModel.ShowMessageRequested += ViewModel_ShowMessageRequested;
            viewModel.ShowConfirmationRequested += ViewModel_ShowConfirmationRequested;
            DataContext = viewModel;
            Unloaded += TasksView_Unloaded;
        }

        private void TasksView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void ViewModel_ShowMessageRequested(object? sender, string message)
        {
            MessageBox.Show(message);
        }

        private void ViewModel_ShowConfirmationRequested(object? sender, (string Message, string Title, Action<bool> Callback) data)
        {
            var result = MessageBox.Show(data.Message, data.Title, MessageBoxButton.YesNo);
            data.Callback(result == MessageBoxResult.Yes);
        }

        private void Toolbar_BackButtonClicked(object sender, System.EventArgs e)
        {
            ViewModel?.GoBack();
        }

        private void Toolbar_AddButtonClicked(object sender, EventArgs e)
        {
            var formPage = new AddEditTasksView(_apiClient, _navigationService, _weddingId);
            _navigationService.NavigateTo(formPage);
        }

        private async void Toolbar_DeleteButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.DeleteTasks();
            }
        }

        private async void PageToolBarView_RefreshButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadDataAsync();
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ViewModel?.SelectedItem != null)
            {
                var formPage = new AddEditTasksView(_apiClient, _navigationService, _weddingId, ViewModel.SelectedItem);
                _navigationService.NavigateTo(formPage);
            }
        }

        private void Toolbar_FirstPageClicked(object sender, EventArgs e)
        {
            ViewModel?.GoToFirstPage();
        }

        private void Toolbar_PreviousPageClicked(object sender, EventArgs e)
        {
            ViewModel?.GoToPreviousPage();
        }

        private void Toolbar_NextPageClicked(object sender, EventArgs e)
        {
            ViewModel?.GoToNextPage();
        }

        private void Toolbar_LastPageClicked(object sender, EventArgs e)
        {
            ViewModel?.GoToLastPage();
        }

        private void DataGrid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var row = FindParent<DataGridRow>(source);

                // If the click was not on a row (e.g., on the empty space),
                // row will be null.
                if (row == null)
                {
                    if (sender is DataGrid grid)
                    {
                        // Clear the selection.
                        grid.UnselectAll();
                    }
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T? parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }
    }
}
