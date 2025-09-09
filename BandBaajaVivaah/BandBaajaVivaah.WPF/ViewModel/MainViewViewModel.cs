using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ApiClientService _apiClient;

        public MainViewModel(ApiClientService apiClient)
        {
            _apiClient = apiClient;
            // We'll add commands here later to load data for the datagrids
        }
    }
}
