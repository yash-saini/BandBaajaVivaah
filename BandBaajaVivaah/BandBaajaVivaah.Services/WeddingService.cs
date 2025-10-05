using BandBaaajaVivaah.Data.Models;
using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTOs;

namespace BandBaajaVivaah.Services
{
    public interface IWeddingService
    {
        Task<WeddingDto?> GetWeddingByIdAsync(int weddingId, int userId);
        Task<IEnumerable<WeddingDto>> GetWeddingsForUserAsync(int userId);
        Task<WeddingDto> CreateWeddingAsync(CreateWeddingDto weddingDto, int ownerUserId);

        Task<bool> DeleteWeddingAsync(int weddingId, int ownerUserId);
        Task<bool> UpdateWeddingAsync(int weddingId, CreateWeddingDto weddingDto, int ownerUserId);

        Task<IEnumerable<WeddingDto>> GetAllWeddingsAsync();
    }

    public class WeddingService : IWeddingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WeddingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<WeddingDto> CreateWeddingAsync(CreateWeddingDto weddingDto, int ownerUserId)
        {
            var wedding = new Wedding
            {
                WeddingName = weddingDto.WeddingName,
                WeddingDate = weddingDto.WeddingDate,
                TotalBudget = weddingDto.TotalBudget,
                OwnerUserId = ownerUserId
            };

            await _unitOfWork.Weddings.AddAsync(wedding);
            await _unitOfWork.CompleteAsync();

            return new WeddingDto
            {
                WeddingID = wedding.WeddingId,
                WeddingName = wedding.WeddingName,
                WeddingDate = wedding.WeddingDate,
                TotalBudget = wedding.TotalBudget,
                OwnerUserId = wedding.OwnerUserId
            };
        }

        public async Task<WeddingDto?> GetWeddingByIdAsync(int weddingId, int userId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);

            if (wedding == null || wedding.OwnerUserId != userId)
            {
                return null; // Not found or user does not have access
            }

            return new WeddingDto
            {
                WeddingID = wedding.WeddingId,
                WeddingName = wedding.WeddingName,
                WeddingDate = wedding.WeddingDate,
                TotalBudget = wedding.TotalBudget,
                OwnerUserId = wedding.OwnerUserId
            };
        }

        public async Task<IEnumerable<WeddingDto>> GetWeddingsForUserAsync(int userId)
        {
            var weddings = await _unitOfWork.Weddings.GetAllForUserAsync(userId);
            return weddings.Select(w => new WeddingDto
            {
                WeddingID = w.WeddingId,
                WeddingName = w.WeddingName,
                WeddingDate = w.WeddingDate,
                TotalBudget = w.TotalBudget,
                OwnerUserId = w.OwnerUserId
            });
        }

        public async Task<bool> DeleteWeddingAsync(int weddingId, int ownerUserId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);
            if (wedding == null || wedding.OwnerUserId != ownerUserId)
            {
                return false; // Not found or user does not have access
            }
            await _unitOfWork.Weddings.DeleteAsync(weddingId);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> UpdateWeddingAsync(int weddingId, CreateWeddingDto weddingDto, int ownerUserId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);
            if (wedding == null || wedding.OwnerUserId != ownerUserId)
            {
                return false; // Not found or user does not have access
            }
            wedding.WeddingName = weddingDto.WeddingName;
            wedding.WeddingDate = weddingDto.WeddingDate;
            wedding.TotalBudget = weddingDto.TotalBudget;
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<IEnumerable<WeddingDto>> GetAllWeddingsAsync()
        {
            var weddings = await _unitOfWork.Weddings.GetAllAsync();
            return weddings.Select(w => new WeddingDto
            {
                WeddingID = w.WeddingId,
                WeddingName = w.WeddingName,
                WeddingDate = w.WeddingDate,
                TotalBudget = w.TotalBudget,
                OwnerUserId = w.OwnerUserId
            });
        }
    }
}