using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Windows;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class GuestsViewModel : PageViewModel<GuestDto>
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly int _weddingId;

        private ObservableCollection<GuestDto> _guests;
        public ObservableCollection<GuestDto> Guests
        {
            get => _guests;
            set
            {
                _guests = value;
                OnPropertyChanged(nameof(Guests));
            }
        }

        private string _searchFirstName;
        public string SearchFirstName
        {
            get => _searchFirstName;
            set
            {
                _searchFirstName = value;
                OnPropertyChanged(nameof(SearchFirstName));
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }
        private string _searchLastName;
        public string SearchLastName
        {
            get => _searchLastName;
            set
            {
                _searchLastName = value;
                OnPropertyChanged(nameof(SearchLastName));
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        private string _side;
        public string Side
        {
            get => _side;
            set
            {
                _side = value;
                OnPropertyChanged(nameof(Side));
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        private string _rsvpStatus;
        public string RSVPStatus
        {
            get => _rsvpStatus;
            set
            {
                _rsvpStatus = value;
                OnPropertyChanged(nameof(RSVPStatus));
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        public GuestsViewModel(ApiClientService apiClient, NavigationService navigationService, int weddingId)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _weddingId = weddingId;
            LoadDataAsync();
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public override async Task LoadDataAsync()
        {
            var guestsList = await _apiClient.GetGuestsAsync(_weddingId);
            if (guestsList != null)
            {
                AllItems = guestsList.ToList();
                Guests = new ObservableCollection<GuestDto>(AllItems);
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        protected override IEnumerable<GuestDto> ApplyFiltering(IEnumerable<GuestDto> items)
        {
            var filtered = items;
            if (!string.IsNullOrWhiteSpace(SearchFirstName))
            {
                filtered = filtered.Where(g => g.FirstName != null && g.FirstName.StartsWith(SearchFirstName, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(SearchLastName))
            {
                filtered = filtered.Where(g => g.LastName != null && g.LastName.StartsWith(SearchLastName, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(Side))
            {
                filtered = filtered.Where(g => g.Side != null && g.Side.StartsWith(Side, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(RSVPStatus))
            {
                filtered = filtered.Where(g => g.RSVPStatus != null && g.RSVPStatus.StartsWith(RSVPStatus, StringComparison.OrdinalIgnoreCase));
            }
            return filtered;
        }

        public async Task DeleteGuest()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Please select a Guest to delete.");
                return;
            }
            var result = MessageBox.Show($"Are you sure you want to delete the Guest - {SelectedItem.FirstName} {SelectedItem.LastName}?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var success = await _apiClient.DeleteGuestAsync(SelectedItem.GuestID);
                if (success)
                {
                    await LoadDataAsync(); // Reload all data
                    SelectedItem = null;
                }
                else
                {
                    MessageBox.Show("Failed to delete the Guest. Please try again.");
                }
            }
        }

    }
}
