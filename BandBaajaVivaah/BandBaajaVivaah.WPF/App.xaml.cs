using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel;
using BandBaajaVivaah.WPF.Views;
using System.Windows;
using System.Windows.Controls;

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

            var apiClient = new ApiClientService();
            var navigationService = new NavigationService();
            var themeService = new ThemeService();
            var guestUpdateService = new GuestUpdateService("https://localhost:7159");
            var expenseUpdateService = new ExpenseUpdateService("https://localhost:7159");

            // This loop will continue until the user closes the app without logging out
            while (true)
            {
                var loginViewModel = new LoginViewModel(apiClient);
                var loginView = new LoginView
                {
                    DataContext = loginViewModel
                };

                // Setup the property changed handler to close the window
                loginViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(loginViewModel.IsLoginSuccessful) && loginViewModel.IsLoginSuccessful)
                    {
                        loginView.Close();
                    }
                };

                loginView.ShowDialog();

                // If the login was successful...
                if (loginViewModel.IsLoginSuccessful)
                {
                    var mainViewModel = new MainViewModel(apiClient, navigationService, loginViewModel.Email, themeService,loginViewModel.Role);
                    var mainView = new MainWindow
                    {
                        DataContext = mainViewModel
                    };

                    // Set the CloseWindow action on the ViewModel
                    mainViewModel.CloseWindow = () => mainView.Close();

                    navigationService.Initialize((Frame)mainView.FindName("MainFrame"));
                    navigationService.NavigateTo(new Uri("/Views/Pages/HomeView.xaml", UriKind.Relative));

                    // Show the main window as a DIALOG. This blocks the loop until it's closed.
                    mainView.ShowDialog();

                    // After MainWindow closes, check if it was due to logout.
                    // If it was, the loop continues and the LoginView is shown again.
                    if (mainViewModel.IsLoggingOut)
                    {
                        continue;
                    }
                    else
                    {
                        // If the user clicked 'X', break the loop and shut down.
                        break;
                    }
                }
                else
                {
                    // If login was not successful (user closed LoginView), break and shut down.
                    break;
                }
            }

            // If the loop ever breaks, the application shuts down.
            Shutdown();
        }
    }

}
