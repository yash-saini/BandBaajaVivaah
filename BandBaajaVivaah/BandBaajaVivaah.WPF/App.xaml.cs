using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel;
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

            // 1. Create a SINGLE ApiClientService that the whole app will share.
            var apiClient = new ApiClientService();

            // 2. Create the LoginViewModel and give it the shared ApiClient.
            var loginViewModel = new LoginViewModel(apiClient);

            // 3. Create the LoginView and set its DataContext.
            var loginView = new LoginView
            {
                DataContext = loginViewModel
            };

            loginViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(loginViewModel.IsLoginSuccessful) && loginViewModel.IsLoginSuccessful)
                {
                    loginView.Close();
                }
            };

            // 4. Show the login window. This line will PAUSE until the login window is closed.
            loginView.ShowDialog();

            // 5. AFTER the window is closed, check the ViewModel's property to see if login was successful.
            if (loginViewModel.IsLoginSuccessful)
            {
                // If it was, create the MainViewModel, giving it the SAME ApiClient that has the token.
                var mainViewModel = new MainViewModel(apiClient);

                var mainView = new MainWindow
                {
                    DataContext = mainViewModel
                };
                mainView.Show();
            }
            else
            {
                // If login was not successful, shut down the app.
                Shutdown();
            }
        }
    }

}
