using System.Windows;

namespace BandBaajaVivaah.WPF.Views
{
    /// <summary>
    /// Interaction logic for RegisterView.xaml
    /// </summary>
    public partial class RegisterView : Window
    {
        public RegisterView()
        {
            InitializeComponent();
        }

        private void BackToLoginLink_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
