using BandBaajaVivaah.WPF.Commands;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Windows.Input;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class ToolbarViewModel : ViewModelBase
    {
        private readonly ThemeService _themeService;
        public string CurrentUserName { get; }
        public ICommand LogoutCommand { get; }
        public Action? CloseWindow { get; set; }

        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged(nameof(IsDarkMode));
                    // When the toggle is flipped, call the service to change the theme
                    _themeService.SetTheme(value);
                }
            }
        }

        public ToolbarViewModel(string userName, ThemeService themeService)
        {
            CurrentUserName = userName;
            LogoutCommand = new RelayCommand(_ => Logout());
            _themeService = themeService;
        }

        private void Logout()
        {
            CloseWindow?.Invoke();
        }
    }
}
