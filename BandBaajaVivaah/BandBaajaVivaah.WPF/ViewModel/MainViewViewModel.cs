using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using BandBaajaVivaah.WPF.Views.Pages;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly NavigationService _navigationService;
        private readonly ApiClientService _apiClient;
        public string WelcomeMessage { get; } = "Welcome!";
        public string CurrentUserName { get; }

        public ToolbarViewModel ToolbarViewModel { get; }
        public ICommand NavigateToWeddingsCommand { get; }
        public ICommand NavigateToTasksCommand { get; }
        public ICommand NavigateToGuestsCommand { get; }
        public ICommand NavigateToExpensesCommand { get; }
        public ICommand LogoutCommand { get; }

        public bool IsLoggingOut { get; private set; } = false;
        public Action CloseWindow { get; set; }

        public MainViewModel(ApiClientService apiClient, NavigationService navigationService, string userName)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            CurrentUserName = userName;
            WelcomeMessage = $"Welcome, {userName.Split('@')[0]}!";

            NavigateToWeddingsCommand = new RelayCommand(_ =>
                _navigationService.NavigateTo(new WeddingsView(_apiClient, _navigationService)));

            //NavigateToTasksCommand = new RelayCommand(_ =>
            //     _navigationService.NavigateTo(new Uri("/Views/Pages/TasksView.xaml", UriKind.Relative)));

            //NavigateToGuestsCommand = new RelayCommand(_ =>
            //    _navigationService.NavigateTo(new Uri("/Views/Pages/GuestsView.xaml", UriKind.Relative)));

            //NavigateToExpensesCommand = new RelayCommand(_ =>
            //    _navigationService.NavigateTo(new Uri("/Views/Pages/ExpensesView.xaml", UriKind.Relative)));

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
