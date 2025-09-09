using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public ICommand LoginCommand { get; }

        private bool _isLoginSuccessful;
        public bool IsLoginSuccessful
        {
            get => _isLoginSuccessful;
            private set
            {
                _isLoginSuccessful = value;
                OnPropertyChanged(nameof(IsLoginSuccessful));
            }
        }

        private readonly ApiClientService _apiClient;

        public LoginViewModel(ApiClientService apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin());
        }

        private bool CanLogin()
        {
            return !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password);
        }

        private async Task LoginAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                var loginDto = new UserLoginDto { Email = this.Email, Password = this.Password };
                var token = await _apiClient.LoginAsync(loginDto);

                if (token != null)
                {
                    IsLoginSuccessful = true;
                }
                else
                {
                    ErrorMessage = "Invalid email or password.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
        }
    }
}
