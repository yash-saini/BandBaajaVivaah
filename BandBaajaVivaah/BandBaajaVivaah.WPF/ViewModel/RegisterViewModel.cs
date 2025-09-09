using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class RegisterViewModel : ViewModelBase
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

        private string _fullname;
        public string FullName
        {
            get => _fullname;
            set
            {
                _fullname = value;
                OnPropertyChanged(nameof(FullName));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public ICommand RegisterCommand { get; }

        private readonly ApiClientService _apiClient;
        public RegisterViewModel(ApiClientService apiClient)
        {
            _apiClient = apiClient;
            RegisterCommand = new RelayCommand(async _ => await RegisterAsync());
        }

        private async Task RegisterAsync()
        {
            var registerDto = new UserRegisterDto
            {
                FullName = this.FullName,
                Email = this.Email,
                Password = this.Password
            };

            var result = await _apiClient.RegisterAsync(registerDto);

            switch (result)
            {
                case RegistrationResult.Success:
                    Message = "Registration successful! You can now log in.";
                    break;
                case RegistrationResult.EmailAlreadyExists:
                    Message = "This email address is already in use.";
                    break;
                case RegistrationResult.Failure:
                    Message = "Registration failed. Please try again.";
                    break;
            }
        }
    }
}
