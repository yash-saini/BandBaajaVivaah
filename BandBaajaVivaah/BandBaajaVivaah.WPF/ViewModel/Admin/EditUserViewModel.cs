using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel.Admin
{
    public class EditUserViewModel : ViewModelBase
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly UserDto _user;

        public string FullName => _user.FullName;
        public string Email => _user.Email;

        private string _role;
        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged(nameof(Role));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand GoBackCommand { get; }

        // This event will be used to tell the main user list to refresh its data after a save.
        public event Func<Task> RefreshParentRequested;
        public event Action<string, string> ShowMessageRequested;


        public EditUserViewModel(ApiClientService apiClient, NavigationService navigationService, UserDto user)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _user = user;
            _role = user.Role; // Initialize with the user's current role

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            GoBackCommand = new RelayCommand(_ => GoBack());
        }

        private async Task SaveAsync()
        {
            var success = await _apiClient.UpdateUserRoleAsync(_user.UserID, Role);
            if (success)
            {
                ShowMessageRequested?.Invoke("Success", "User role updated successfully.");
                // Raise the event to refresh the parent grid
                if (RefreshParentRequested != null)
                {
                    await RefreshParentRequested.Invoke();
                }
                GoBack();
            }
            else
            {
                ShowMessageRequested?.Invoke("Error", "Failed to update user role.");
            }
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }
    }
}
