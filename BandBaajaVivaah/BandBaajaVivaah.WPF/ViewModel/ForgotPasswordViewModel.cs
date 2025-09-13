using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class ForgotPasswordViewModel : ViewModelBase
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
        // You can add a command here for sending the reset password email
        public ICommand SendResetEmailCommand { get; }
        private readonly ApiClientService _apiClient;

        public ForgotPasswordViewModel(ApiClientService apiClient)
        {
            _apiClient = apiClient;
            SendResetEmailCommand = new RelayCommand(async _ => await SendResetEmailAsync());
        }

        private async Task SendResetEmailAsync()
        {
            Message = "Sending request...";
            var success = await _apiClient.ForgotPasswordAsync(this.Email);
            if (success)
            {
                Message = "If an account exists, an email has been sent.";
            }
            else
            {
                Message = "Something went wrong. Please try again.";
            }
        }
    }
}
