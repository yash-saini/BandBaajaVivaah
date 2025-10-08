using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel;
using BandBaajaVivaah.WPF.ViewModel.AddEditViewModel;
using System.Windows;
using System.Windows.Controls;

namespace BandBaajaVivaah.WPF.Views.Pages
{
    /// <summary>
    /// Interaction logic for AddEditExpensesView.xaml
    /// </summary>
    public partial class AddEditExpensesView : Page
    {
        AddEditExpensesViewModel? ViewModel => DataContext as AddEditExpensesViewModel;
        private readonly NavigationService _navigationService;
        private readonly ApiClientService _apiClient;
        private ExpensesViewModel? _parentViewModel;
        private readonly int _weddingId;

        public AddEditExpensesView(ApiClientService apiClient, NavigationService navigationService, int weddingId, ExpenseDto expense = null)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _navigationService = navigationService;
            _weddingId = weddingId;

            // Store a reference to the parent ViewModel if it's available
            if (navigationService.GetPreviousPage() is ExpensesView expensesView &&
                expensesView.DataContext is ExpensesViewModel expensesViewModel)
            {
                _parentViewModel = expensesViewModel;
            }

            // Create the view model with weddingId
            var viewModel = new AddEditExpensesViewModel(apiClient, navigationService, _weddingId, expense);

            // Hook up the refresh event
            viewModel.RefreshParentRequested += RefreshExpensesData;

            this.DataContext = viewModel;
        }

        private void Toolbar_BackButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel != null && ViewModel.HasErrors)
            {
                var result = MessageBox.Show("There are validation errors. Are you sure you want to discard your changes?",
                                             "Confirm Navigation",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    ViewModel.GoBack();
                }
            }
            else
            {
                ViewModel?.GoBack();
            }
        }

        private async void Toolbar_SaveButtonClicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.SaveAsync();
            }
        }

        private async Task RefreshExpensesData()
        {
            if (_parentViewModel != null)
            {
                await _parentViewModel.LoadDataAsync();
                return;
            }

            var expensesViewModel = new ExpensesViewModel(_apiClient, _navigationService, _weddingId);
            await expensesViewModel.LoadDataAsync();
        }
    }
}