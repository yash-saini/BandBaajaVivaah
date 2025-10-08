using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Admin;
using System.Windows;
using System.Windows.Controls;
using NavigationService = BandBaajaVivaah.WPF.Services.NavigationService;

namespace BandBaajaVivaah.WPF.Views.Pages.Admin
{
    /// <summary>
    /// Interaction logic for EditUserView.xaml
    /// </summary>
    public partial class EditUserView : Page
    {
        private readonly AdminUsersViewModel _parentViewModel;
        EditUserViewModel ViewModel => DataContext as EditUserViewModel;

        public EditUserView(ApiClientService apiClient, NavigationService navigationService, UserDto userToEdit, AdminUsersViewModel parentViewModel)
        {
            InitializeComponent();
            _parentViewModel = parentViewModel;

            var viewModel = new EditUserViewModel(apiClient, navigationService, userToEdit);
            viewModel.RefreshParentRequested += OnRefreshParentRequested;
            viewModel.ShowMessageRequested += (title, msg) => MessageBox.Show(msg, title);

            DataContext = viewModel;
        }

        private async Task OnRefreshParentRequested()
        {
            if (_parentViewModel != null)
            {
                await _parentViewModel.LoadDataAsync();
            }
        }

        private void Toolbar_BackButtonClicked(object sender, System.EventArgs e) => ViewModel?.GoBackCommand.Execute(null);
        private void Toolbar_SaveButtonClicked(object sender, System.EventArgs e) => ViewModel?.SaveCommand.Execute(null);
    }
}
