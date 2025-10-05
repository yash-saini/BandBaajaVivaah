using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel.Admin
{
    public class AdminUsersViewModel : PageViewModel<UserDto>
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;

        public ICommand DeleteUserCommand { get; }
        public ICommand ViewUserWeddingsCommand { get; }

        public event EventHandler<string> ShowMessageRequested;
        public event EventHandler<(string Message, string Title, Action<bool> Callback)> ShowConfirmationRequested;

        public AdminUsersViewModel(ApiClientService apiClient, NavigationService navigationService)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            DeleteUserCommand = new RelayCommand(async _ => await DeleteSelectedUser(), _ => SelectedItem != null);
            ViewUserWeddingsCommand = new RelayCommand(ViewUserWeddings, param => param is UserDto);


            LoadDataAsync();
        }

        public async override Task LoadDataAsync()
        {
            var users = await _apiClient.GetAllUsersAsync();
            if (users != null)
            {
                AllItems = users.ToList();
                UpdateDisplayedItems();
            }
        }

        private async Task DeleteSelectedUser()
        {
            if (SelectedItem == null)
            {
                ShowMessageRequested?.Invoke(this, "Please select a user to delete.");
                return;
            }

            var userToDelete = SelectedItem;

            ShowConfirmationRequested?.Invoke(this,
                ($"Are you sure you want to delete the user '{userToDelete.FullName}'?", "Confirm Delete",
                async (confirmed) =>
                {
                    if (confirmed)
                    {
                        var success = await _apiClient.DeleteUserAsync(userToDelete.UserID);
                        if (success)
                        {
                            ShowMessageRequested?.Invoke(this, "User deleted successfully.");
                            await LoadDataAsync();
                        }
                        else
                        {
                            ShowMessageRequested?.Invoke(this, "Failed to delete user. You cannot delete yourself, or an error occurred.");
                        }
                    }
                }
            ));
        }

        private void ViewUserWeddings(object userObject)
        {
            if (userObject is UserDto user)
            {
                // We will create this view next
                // _navigationService.NavigateTo(new AdminUserWeddingsView(_apiClient, _navigationService, user));
                ShowMessageRequested?.Invoke(this, $"Navigating to weddings for {user.FullName}. (View not implemented yet)");
            }
        }

        public void GoBack() => _navigationService.GoBack();

        protected override IEnumerable<UserDto> ApplyFiltering(IEnumerable<UserDto> items)
        {
            // No filtering implemented yet, return all items
            return items;
        }
    }
}
