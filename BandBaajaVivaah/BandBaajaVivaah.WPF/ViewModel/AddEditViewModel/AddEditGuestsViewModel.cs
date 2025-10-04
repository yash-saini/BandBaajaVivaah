using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.WPF.Services;
using BandBaajaVivaah.WPF.ViewModel.Base;
using System.Windows;

namespace BandBaajaVivaah.WPF.ViewModel.AddEditViewModel
{
    public class AddEditGuestsViewModel : ValidatableViewModelBase
    {
        private readonly ApiClientService _apiClient;
        private readonly NavigationService _navigationService;
        private readonly GuestDto? _editingGuests;
        private readonly int _weddingId;

        private bool _formSubmitAttempted = false;

        public bool ShowValidationSummary => _formSubmitAttempted && HasErrors;
        public string Title => _editingGuests == null ? "Add New Guest" : "Edit Guest";

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
                if (_formSubmitAttempted)
                    ValidateFirstName();
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
                if (_formSubmitAttempted)
                    ValidateLastName();
            }
        }

        private string _side = string.Empty;
        public string Side
        {
            get => _side;
            set
            {
                _side = value;
                OnPropertyChanged(nameof(Side));
            }
        }

        private string _rsvpStatus = string.Empty;
        public string RSVPStatus
        {
            get => _rsvpStatus;
            set
            {
                _rsvpStatus = value;
                OnPropertyChanged(nameof(RSVPStatus));
            }
        }

        public bool CanSave => !HasErrors;

        public bool WasSuccess { get; private set; } = false;

        public event Func<Task> RefreshParentRequested;

        public AddEditGuestsViewModel(ApiClientService apiClient, NavigationService navigationService, int weddingId, GuestDto? guest = null)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _editingGuests = guest;
            _weddingId = weddingId;

            if (_editingGuests != null)
            {
                // EDIT MODE
                FirstName = _editingGuests.FirstName;
                LastName = _editingGuests.LastName;
                Side = _editingGuests.Side;
                RSVPStatus = _editingGuests.RSVPStatus;
            }
            else
            {
                // ADD MODE
                FirstName = string.Empty;
                LastName = string.Empty;
                Side = "Bride";
                RSVPStatus = "No Response";
            }
            ErrorsChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(ShowValidationSummary));
            };
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public async Task SaveAsync()
        {
            _formSubmitAttempted = true;
            ValidateFirstName();
            ValidateLastName();
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(ShowValidationSummary));

            if (HasErrors)
            {
                return;
            }

            var dto = new CreateGuestDto
            {
                FirstName = FirstName,
                LastName = LastName,
                Side = Side,
                RSVPStatus = RSVPStatus,
                WeddingID = _weddingId
            };

            bool success;
            if (_editingGuests == null) // Add Mode
            {
                var newGuest = await _apiClient.CreateGuestAsync(dto);
                success = newGuest != null;
            }
            else // Edit Mode
            {
                success = await _apiClient.UpdateGuestAsync(_editingGuests.GuestID, dto);
            }

            if (success)
            {
                WasSuccess = true;
                await (RefreshParentRequested?.Invoke() ?? Task.CompletedTask);

                _navigationService.GoBack();
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        private void ValidateFirstName()
        {
            ClearErrors(nameof(FirstName));
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                AddError(nameof(FirstName), "First Name is required.");
            }
            else if (FirstName.Length > 50)
            {
                AddError(nameof(FirstName), "First Name cannot exceed 50 characters.");
            }
        }

        private void ValidateLastName()
        {
            ClearErrors(nameof(LastName));
            if (string.IsNullOrWhiteSpace(LastName))
            {
                AddError(nameof(LastName), "Last Name is required.");
            }
            else if (LastName.Length > 50)
            {
                AddError(nameof(LastName), "Last Name cannot exceed 50 characters.");
            }
        }
    }
}
