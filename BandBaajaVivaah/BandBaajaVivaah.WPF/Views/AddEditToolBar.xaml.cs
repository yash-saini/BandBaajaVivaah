using System.Windows;
using System.Windows.Controls;

namespace BandBaajaVivaah.WPF.Views
{
    /// <summary>
    /// Interaction logic for AddEditToolBar.xaml
    /// </summary>
    public partial class AddEditToolBar : UserControl
    {
        public event EventHandler BackButtonClicked;
        public event EventHandler SaveButtonClicked;
        public AddEditToolBar()
        {
            InitializeComponent();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
