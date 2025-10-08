using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class ToolbarViewModel : ViewModelBase
    {
        public string CurrentUserName { get; }
        public ICommand LogoutCommand { get; }
        public Action? CloseWindow { get; set; }

        public ToolbarViewModel(string userName)
        {
            CurrentUserName = userName;
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        private void Logout()
        {
            CloseWindow?.Invoke();
        }
    }
}
