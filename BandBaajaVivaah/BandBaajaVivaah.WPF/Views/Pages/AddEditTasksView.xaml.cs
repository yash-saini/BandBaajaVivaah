using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel;
using BandBaajaVivaah.WPF.ViewModel.AddEditViewModel;
using System.Windows.Controls;
using NavigationService = BandBaajaVivaah.WPF.Services.NavigationService;

namespace BandBaajaVivaah.WPF.Views.Pages
{
    /// <summary>
    /// Interaction logic for AddEditTasksView.xaml
    /// </summary>
    public partial class AddEditTasksView : Page
    {
        AddEditTasksViewModel? ViewModel => DataContext as AddEditTasksViewModel;
        private readonly NavigationService _navigationService;
        private readonly ApiClientService _apiClient;
        private TasksViewModel? _parentViewModel;
        private readonly int _weddingId;

        public AddEditTasksView(ApiClientService apiClient, NavigationService navigationService, int weddingId, TaskDto task = null)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _navigationService = navigationService;
            _weddingId = weddingId;

            // Store a reference to the parent ViewModel if it's available
            if (navigationService.GetPreviousPage() is TasksView tasksView &&
                tasksView.DataContext is TasksViewModel tasksViewModel)
            {
                _parentViewModel = tasksViewModel;
            }

            // Create the view model with weddingId
            var viewModel = new AddEditTasksViewModel(apiClient, navigationService, _weddingId, task);

            // Hook up the refresh event
            viewModel.RefreshParentRequested += RefreshTasksData;

            this.DataContext = viewModel;
        }

        private void Toolbar_BackButtonClicked(object sender, EventArgs e)
        {
            ViewModel?.GoBack();
        }

        private async void Toolbar_SaveButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.SaveAsync();
            }
        }

        private async Task RefreshTasksData()
        {
            if (_parentViewModel != null)
            {
                await _parentViewModel.LoadDataAsync();
                return;
            }

            var tasksViewModel = new TasksViewModel(_apiClient, _navigationService, _weddingId);
            await tasksViewModel.LoadDataAsync();
        }
    }
}
