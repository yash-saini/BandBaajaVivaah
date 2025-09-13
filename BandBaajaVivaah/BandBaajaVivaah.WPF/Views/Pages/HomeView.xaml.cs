using System.Windows;
using System.Windows.Controls;

namespace BandBaajaVivaah.WPF.Views.Pages
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : Page
    {
        public HomeView()
        {
            InitializeComponent();
            this.DataContext = Application.Current.MainWindow.DataContext;
        }
    }
}
