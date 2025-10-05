using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using BandBaajaVivaah.WPF.Views;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Windows;
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
        public ICommand RegisterCommand { get; }

        public ICommand ForgotPasswordCommand { get; }

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

        public string Role { get; private set; }

        public LoginViewModel(ApiClientService apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin());
            RegisterCommand = new RelayCommand(_ => OpenRegisterWindow());
            ForgotPasswordCommand = new RelayCommand(_ => OpenForgotPasswordWindow());
        }

        private void OpenRegisterWindow()
        {
            var loginWindow = Application.Current.Windows.OfType<LoginView>().FirstOrDefault();
            var registerViewModel = new RegisterViewModel(_apiClient);
            var registerView = new RegisterView
            {
                DataContext = registerViewModel,
                Owner = loginWindow
            };
            registerView.ShowDialog();
        }

        private void OpenForgotPasswordWindow()
        {
            var viewModel = new ForgotPasswordViewModel(_apiClient);
            var view = new ForgotPasswordView
            {
                DataContext = viewModel
            };
            view.ShowDialog();
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
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    Role = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value;

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
