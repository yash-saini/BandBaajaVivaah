using System.Windows.Controls;

namespace BandBaajaVivaah.WPF.Services
{
    public class NavigationService
    {
        private Frame _mainFrame;

        public void Initialize(Frame frame)
        {
            _mainFrame = frame;
        }

        public void NavigateTo(Uri pageUri)
        {
            _mainFrame.Navigate(pageUri);
        }

        public void NavigateTo(Page page)
        {
            _mainFrame.Navigate(page);
        }

        public void GoBack()
        {
            if (_mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
            }
        }
    }
}
