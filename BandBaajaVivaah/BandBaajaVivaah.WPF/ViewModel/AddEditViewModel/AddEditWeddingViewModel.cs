using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Windows;

namespace BandBaajaVivaah.WPF.ViewModel.AddEditViewModel
{
    public class AddEditWeddingViewModel : ViewModelBase
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly WeddingDto? _editingWedding;

        public string Title => _editingWedding == null ? "Add New Wedding" : "Edit Wedding";

        private string _weddingName = string.Empty;
        public string WeddingName
        {
            get => _weddingName;
            set
            {
                _weddingName = value;
                OnPropertyChanged(nameof(WeddingName));
            }
        }

        private DateTime? _weddingDate = DateTime.Now;
        public DateTime? WeddingDate
        {
            get => _weddingDate;
            set
            {
                _weddingDate = value;
                OnPropertyChanged(nameof(WeddingDate));
            }
        }

        private decimal? _totalBudget = 0;
        public decimal? TotalBudget
        {
            get => _totalBudget;
            set
            {
                _totalBudget = value;
                OnPropertyChanged(nameof(TotalBudget));
            }
        }

        public bool WasSuccess { get; private set; } = false;

        public AddEditWeddingViewModel(ApiClientService apiClient, NavigationService navigationService, WeddingDto? wedding = null)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _editingWedding = wedding;

            if (_editingWedding != null)
            {
                // EDIT MODE: Populate the form with the existing wedding's data
                WeddingName = _editingWedding.WeddingName;
                WeddingDate = _editingWedding.WeddingDate;
                TotalBudget = _editingWedding.TotalBudget;
            }
            else
            {
                // ADD MODE
                WeddingDate = DateTime.Now;
                TotalBudget = 0;
                WeddingName = string.Empty;
            }
        }

        public async Task SaveAsync()
        {
            var dto = new CreateWeddingDto
            {
                WeddingName = this.WeddingName,
                WeddingDate = this.WeddingDate.HasValue ? this.WeddingDate.Value : DateTime.Today,
                TotalBudget = this.TotalBudget.HasValue ? this.TotalBudget.Value : 0
            };

            bool success;
            if (_editingWedding == null) // Add Mode
            {
                var newWedding = await _apiClient.CreateWeddingAsync(dto);
                success = newWedding != null;
            }
            else // Edit Mode
            {
                success = await _apiClient.UpdateWeddingAsync(_editingWedding.WeddingID, dto);
            }

            if (success)
            {
                WasSuccess = true;
                _navigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Failed to save. Please try again.");
            }
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }
    }
}
