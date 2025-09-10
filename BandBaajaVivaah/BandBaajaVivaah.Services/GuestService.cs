using BandBaaajaVivaah.Data.Models;
using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTOs;

namespace BandBaajaVivaah.Services
{
    public interface IGuestService
    {
        Task<IEnumerable<GuestDto>> GetGuestsByWeddingIdAsync(int weddingId, int userId);
        Task<GuestDto> CreateGuestAsync(CreateGuestDto guestDto, int userId);
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

            return new GuestDto
            {
                GuestID = guest.GuestId,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                Side = guest.Side,
                RSVPStatus = guest.Rsvpstatus
            };
        }

        public async Task<IEnumerable<GuestDto>> GetGuestsByWeddingIdAsync(int weddingId, int userId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);
            if (wedding == null || wedding.OwnerUserId != userId)
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
    }
}
