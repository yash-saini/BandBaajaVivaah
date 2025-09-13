using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class ResetPasswordViewModel : ViewModelBase
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
        public string NewPassword
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(NewPassword));
            }
        }

        private string _token;
        public string Token
        {
            get => _token;
            set
            {
                _token = value;
                OnPropertyChanged(nameof(Token));
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

        public ICommand ResetPasswordCommand { get; }

        private readonly ApiClientService _apiClient;

        public ResetPasswordViewModel(ApiClientService apiClient, string email)
        {
            _apiClient = apiClient;
            Email = email; 
            ResetPasswordCommand = new RelayCommand(async _ => await ResetPasswordAsync());
        }

        private async Task ResetPasswordAsync()
        {
            var resetDto = new ResetPasswordDto
            {
                Email = this.Email,
                Token = this.Token,
                NewPassword = this.NewPassword
            };

            var success = await _apiClient.ResetPasswordAsync(resetDto);
            if (success)
            {
                Message = "Password has been reset successfully! You can now close this window and log in.";
            }
            else
            {
                Message = "Password reset failed. The token may be invalid or expired.";
            }
        }
    }
}
