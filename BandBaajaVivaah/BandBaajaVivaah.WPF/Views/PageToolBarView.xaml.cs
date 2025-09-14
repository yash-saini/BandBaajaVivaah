using System.Windows;
using System.Windows.Controls;

namespace BandBaajaVivaah.WPF.Views
{
    /// <summary>
    /// Interaction logic for PageToolBarView.xaml
    /// </summary>
    public partial class PageToolBarView : UserControl
    {
        public event EventHandler? BackButtonClicked;
        public event EventHandler? AddButtonClicked;
        public event EventHandler? DeleteButtonClicked;
        public event EventHandler? RefreshButtonClicked;
        public Button DeleteButtonControl => DeleteButton;

        public PageToolBarView()
        {
            InitializeComponent();
            DeleteButton.IsEnabled = false;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        { 
            RefreshButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
