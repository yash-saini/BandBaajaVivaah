using BandBaaajaVivaah.Data.Models;
using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.Grpc;
using BandBaajaVivaah.Services.GrpcServices;
using Task = System.Threading.Tasks.Task;

namespace BandBaajaVivaah.Services
{
    public interface IGuestService
    {
        Task<IEnumerable<GuestDto>> GetGuestsByWeddingIdAsync(int weddingId, int userId);
        Task<GuestDto> CreateGuestAsync(CreateGuestDto guestDto, int userId);
        Task<bool> UpdateGuestAsync(int guestId, CreateGuestDto updateDto, int userId);
        Task<bool> DeleteGuestAsync(int guestId, int userId);

        Task<GuestDto> CreateGuestAsAdminAsync(CreateGuestDto guestDto);
        Task<bool> UpdateGuestAsAdminAsync(int guestId, CreateGuestDto updateDto);
        Task<bool> DeleteGuestAsAdminAsync(int guestId);
    }

    public class GuestService : IGuestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GuestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GuestDto> CreateGuestAsync(CreateGuestDto guestDto, int userId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(guestDto.WeddingID);
            if (wedding == null || wedding.OwnerUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to add a guest to this wedding.");
            }
            return await CreateGuestAsAdminAsync(guestDto);
        }

        public async Task<GuestDto> CreateGuestAsAdminAsync(CreateGuestDto guestDto)
        {
            var guest = new Guest
            {
                FirstName = guestDto.FirstName,
                LastName = guestDto.LastName,
                Side = guestDto.Side,
                Rsvpstatus = guestDto.RSVPStatus,
                WeddingId = guestDto.WeddingID
            };
            await _unitOfWork.Guests.AddAsync(guest);
            await _unitOfWork.CompleteAsync();
            var result = new GuestDto
            {
                GuestID = guest.GuestId,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                Side = guest.Side,
                RSVPStatus = guest.Rsvpstatus
            };

            // Notify subscribers about the new guest
            await NotifyGuestChange(guest, GuestUpdateEvent.Types.UpdateType.Created);

            return result;
        }

        public async Task<IEnumerable<GuestDto>> GetGuestsByWeddingIdAsync(int weddingId, int userId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            bool isAdmin = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            if (wedding == null || (!isAdmin && wedding.OwnerUserId != userId))
            {
                throw new UnauthorizedAccessException("You are not authorized to view guests for this wedding.");
            }

            var guests = await _unitOfWork.Guests.FindAllAsync(g => g.WeddingId == weddingId);
            return guests.Select(g => new GuestDto
            {
                GuestID = g.GuestId,
                FirstName = g.FirstName,
                LastName = g.LastName,
                Side = g.Side,
                RSVPStatus = g.Rsvpstatus
            });
        }

        public async Task<bool> UpdateGuestAsync(int guestId, CreateGuestDto updateDto, int userId)
        {
            var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
            if (guest == null) return false;

            var wedding = await _unitOfWork.Weddings.GetByIdAsync(guest.WeddingId);
            if (wedding == null || wedding.OwnerUserId != userId)
            {
                return false; // User does not own the wedding this guest belongs to
            }
            return await UpdateGuestAsAdminAsync(guestId, updateDto);
        }

        public async Task<bool> UpdateGuestAsAdminAsync(int guestId, CreateGuestDto updateDto)
        {
            var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
            if (guest == null) return false;

            guest.FirstName = updateDto.FirstName;
            guest.LastName = updateDto.LastName;
            guest.Side = updateDto.Side;
            guest.Rsvpstatus = updateDto.RSVPStatus;
            await _unitOfWork.CompleteAsync();
            await NotifyGuestChange(guest, GuestUpdateEvent.Types.UpdateType.Updated);

            return true;
        }

        public async Task<bool> DeleteGuestAsync(int guestId, int userId)
        {
            var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
            if (guest == null) return false;

            var wedding = await _unitOfWork.Weddings.GetByIdAsync(guest.WeddingId);
            if (wedding == null || wedding.OwnerUserId != userId)
            {
                return false;
            }
            return await DeleteGuestAsAdminAsync(guestId); // Reuse admin logic after check
        }

        public async Task<bool> DeleteGuestAsAdminAsync(int guestId)
        {
            var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
            if (guest == null) return false;

            // Store wedding ID and guest details before deletion for notification
            var weddingId = guest.WeddingId;
            var guestDetails = CreateGuestDetails(guest);

            await _unitOfWork.Guests.DeleteAsync(guestId);
            await _unitOfWork.CompleteAsync();

            // Notify subscribers about the deleted guest
            await GuestUpdateGrpcService.NotifyGuestChange(
                weddingId,
                GuestUpdateEvent.Types.UpdateType.Deleted,
                guestDetails);

            return true;
        }

        private async Task NotifyGuestChange(Guest guest, GuestUpdateEvent.Types.UpdateType updateType)
        {
            var guestDetails = CreateGuestDetails(guest);

            await GuestUpdateGrpcService.NotifyGuestChange(
                guest.WeddingId,
                updateType,
                guestDetails);
        }

        // Helper to create gRPC GuestDetails object from Guest model
        private GuestDetails CreateGuestDetails(Guest guest)
        {
            return new GuestDetails
            {
                GuestId = guest.GuestId,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                Side = guest.Side,
                RsvpStatus = guest.Rsvpstatus,
                WeddingId = guest.WeddingId
            };
        }
    }
}
