using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using BandBaajaVivaah.WPF.Views.Pages;
using BandBaajaVivaah.WPF.Views.Pages.Admin;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly NavigationService _navigationService;
        private readonly ApiClientService _apiClient;
        public string WelcomeMessage { get; } = "Welcome!";
        public string CurrentUserName { get; }
        public bool IsAdmin { get; }

        public ToolbarViewModel ToolbarViewModel { get; }
        public ICommand NavigateToWeddingsCommand { get; }
        public ICommand NavigateToAdminCommand { get; }
        public ICommand LogoutCommand { get; }

        public bool IsLoggingOut { get; private set; } = false;
        public Action CloseWindow { get; set; }

        public MainViewModel(ApiClientService apiClient, NavigationService navigationService, string userName, string role = "User")
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            CurrentUserName = userName;
            WelcomeMessage = $"Welcome, {userName.Split('@')[0]}!";

            IsAdmin = role == "Admin";

            NavigateToWeddingsCommand = new RelayCommand(_ =>
                _navigationService.NavigateTo(new WeddingsView(_apiClient, _navigationService)));

            NavigateToAdminCommand = new RelayCommand(
                _ => _navigationService.NavigateTo(new AdminUsersView(_apiClient, _navigationService)),
                _ => IsAdmin);


            ToolbarViewModel = new ToolbarViewModel(userName);
            ToolbarViewModel.CloseWindow = () => Logout();
        }

        private void Logout()
        {
            IsLoggingOut = true;
            CloseWindow?.Invoke();
        }
    }
}
