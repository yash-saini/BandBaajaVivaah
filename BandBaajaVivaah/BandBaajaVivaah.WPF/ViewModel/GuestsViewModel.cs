using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.Grpc;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using GuestUpdateService = BandBaajaVivaah.WPF.Services.GuestUpdateService;

namespace BandBaajaVivaah.WPF.ViewModel
{
    public class GuestsViewModel : PageViewModel<GuestDto>, IDisposable
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly int _weddingId;
        private readonly GuestUpdateService _guestUpdateService;
        private bool _isSubscribed = false;

        public event EventHandler<string> ShowMessageRequested;
        public event EventHandler<(string Message, string Title, Action<bool> Callback)> ShowConfirmationRequested;

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

        public GuestsViewModel(ApiClientService apiClient, NavigationService navigationService, int weddingId, GuestUpdateService guestUpdateService = null)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _weddingId = weddingId;
            _guestUpdateService = guestUpdateService;

            if (_guestUpdateService != null)
            {
                SubscribeToGuestUpdates();
            }

            LoadDataAsync();
        }

        private async void SubscribeToGuestUpdates()
        {
            if (_isSubscribed)
                return;

            try
            {
                _guestUpdateService.OnGuestUpdate += OnGuestUpdateReceived;
                await _guestUpdateService.SubscribeToWeddingUpdates(_weddingId);
                _isSubscribed = true;
            }
            catch (Exception ex)
            {
                ShowMessageRequested?.Invoke(this, $"Failed to subscribe to real-time updates: {ex.Message}");
            }
        }

        private void OnGuestUpdateReceived(object sender, GuestUpdateEvent e)
        {
            // Use dispatcher to ensure UI updates happen on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                var guestDto = new GuestDto
                {
                    GuestID = e.Guest.GuestId,
                    FirstName = e.Guest.FirstName,
                    LastName = e.Guest.LastName,
                    Side = e.Guest.Side,
                    RSVPStatus = e.Guest.RsvpStatus
                };

                switch (e.Type)
                {
                    case GuestUpdateEvent.Types.UpdateType.Created:
                        HandleGuestCreated(guestDto);
                        break;
                    case GuestUpdateEvent.Types.UpdateType.Updated:
                        HandleGuestUpdated(guestDto);
                        break;
                    case GuestUpdateEvent.Types.UpdateType.Deleted:
                        HandleGuestDeleted(guestDto);
                        break;
                }
            });
        }

        private void HandleGuestCreated(GuestDto guest)
        {
            if (AllItems.Any(g => g.GuestID == guest.GuestID))
                return;

            AllItems.Add(guest);
            UpdateDisplayedItems();
            ShowMessageRequested?.Invoke(this, $"New guest added: {guest.FirstName} {guest.LastName}");
        }

        private void HandleGuestUpdated(GuestDto guest)
        {
            var existingGuest = AllItems.FirstOrDefault(g => g.GuestID == guest.GuestID);
            if (existingGuest != null)
            {
                var index = AllItems.IndexOf(existingGuest);
                AllItems[index] = guest;
                UpdateDisplayedItems();
            }
        }

        private void HandleGuestDeleted(GuestDto guest)
        {
            var existingGuest = AllItems.FirstOrDefault(g => g.GuestID == guest.GuestID);
            if (existingGuest != null)
            {
                AllItems.Remove(existingGuest);
                UpdateDisplayedItems();
            }
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
                ShowMessageRequested?.Invoke(this, "Please select a Guest to delete.");
                return;
            }

            ShowConfirmationRequested?.Invoke(this,
                ($"Are you sure you want to delete {SelectedItem.FirstName}?", "Confirm Delete",
                async (confirmed) =>
                {
                    if (confirmed)
                    {
                        // Always call the standard DeleteGuestAsync.
                        var success = await _apiClient.DeleteGuestAsync(SelectedItem.GuestID);
                        if (success)
                        {
                            await LoadDataAsync();
                        }
                        else
                        {
                            ShowMessageRequested?.Invoke(this, "Failed to delete the Guest.");
                        }
                    }
                }
            ));
        }

        public void Dispose()
        {
            if (_guestUpdateService != null && _isSubscribed)
            {
                _guestUpdateService.OnGuestUpdate -= OnGuestUpdateReceived;
                _guestUpdateService.Unsubscribe();
                _isSubscribed = false;
            }
        }
    }
}