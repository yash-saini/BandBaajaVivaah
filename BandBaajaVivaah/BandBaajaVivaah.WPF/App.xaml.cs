using BandBaajaVivaah.WPF.Views;
using System.Windows;

namespace BandBaajaVivaah.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginView = new LoginView();
            loginView.ShowDialog();

            // This logic will be moved into the LoginViewModel
            if (loginView.DialogResult == true)
            {
                var mainView = new MainWindow();
                mainView.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }

}
