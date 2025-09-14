using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.AddEditViewModel;
using System.Windows.Controls;

namespace BandBaajaVivaah.WPF.Views.Pages
{
    /// <summary>
    /// Interaction logic for AddEditWeddingsView.xaml
    /// </summary>
    public partial class AddEditWeddingsView : Page
    {
        AddEditWeddingViewModel? ViewModel => DataContext as AddEditWeddingViewModel;

        public AddEditWeddingsView(ApiClientService apiClient, NavigationService navigationService, WeddingDto wedding = null)
        {
            InitializeComponent();
            this.DataContext = new AddEditWeddingViewModel(apiClient, navigationService, wedding);
        }

        private void Toolbar_BackButtonClicked(object sender, EventArgs e)
        {
            ViewModel?.GoBack();
        }

        private async void Toolbar_SaveButtonClicked(object sender, EventArgs e)
        {
            await ViewModel?.SaveAsync();
        }
    }
}
