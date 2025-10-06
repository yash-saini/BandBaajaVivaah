using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NavigationService = BandBaajaVivaah.WPF.Services.NavigationService;

namespace BandBaajaVivaah.WPF.Views.Pages
{
    /// <summary>
    /// Interaction logic for WeddingsView.xaml
    /// </summary>
    public partial class WeddingsView : Page
    {
        WeddingsViewModel? ViewModel => DataContext as WeddingsViewModel;
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;

        public WeddingsView(ApiClientService apiClient, NavigationService navigationService)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _navigationService = navigationService;

            var viewModel = new WeddingsViewModel(apiClient, navigationService);
            SetupViewModelEvents(viewModel);
            DataContext = viewModel;
        }

        // Constructor for admin mode
        public WeddingsView(ApiClientService apiClient, NavigationService navigationService, UserDto targetUser)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _navigationService = navigationService;

            var viewModel = new WeddingsViewModel(apiClient, navigationService, targetUser);
            SetupViewModelEvents(viewModel);
            DataContext = viewModel;
        }

        private void SetupViewModelEvents(WeddingsViewModel viewModel)
        {
            viewModel.ShowMessageRequested += (s, msg) => MessageBox.Show(msg);
            viewModel.ShowConfirmationRequested += (s, data) =>
            {
                var result = MessageBox.Show(data.Message, data.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                data.Callback(result == MessageBoxResult.Yes);
            };
        }

        private void Toolbar_BackButtonClicked(object sender, EventArgs e)
        {
            ViewModel?.GoBack();
        }

        private void Toolbar_AddButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel?.AddWeddingCommand != null)
            {
                ViewModel.AddWeddingCommand.Execute(null);
            }
        }

        private async void Toolbar_DeleteButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.DeleteWedding();
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
                // Check if this is admin mode and pass targetUserId if it is
                if (ViewModel.IsAdminMode && ViewModel.TargetUser != null)
                {
                    var formPage = new AddEditWeddingsView(
                        _apiClient,
                        _navigationService,
                        ViewModel.SelectedItem,
                        ViewModel.TargetUser.UserID); // Pass targetUserId for admin mode
                    _navigationService.NavigateTo(formPage);
                }
                else
                {
                    var formPage = new AddEditWeddingsView(_apiClient, _navigationService, ViewModel.SelectedItem);
                    _navigationService.NavigateTo(formPage);
                }
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
