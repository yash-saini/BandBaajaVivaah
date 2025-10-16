using BandBaaajaVivaah.Data.Models;
using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.Grpc;
using BandBaajaVivaah.Services.GrpcServices;
using Google.Protobuf.WellKnownTypes;

namespace BandBaajaVivaah.Services
{
    public interface IWeddingService
    {
        Task<IEnumerable<WeddingDto>> GetWeddingsAsync(int userId);
        Task<WeddingDto> CreateWeddingAsync(CreateWeddingDto weddingDto, int userId);
        Task<bool> DeleteWeddingAsync(int weddingId, int? ownerUserId);
        Task<bool> UpdateWeddingAsync(int weddingId, CreateWeddingDto weddingDto, int? ownerUserId);
        Task<WeddingDto?> GetWeddingByIdAsync(int weddingId, int? ownerUserId);
        Task<IEnumerable<WeddingDto>> GetAllWeddingsAsync();
        Task<IEnumerable<WeddingDto>> GetWeddingsForUserAsync(int userId);
    }

    public class WeddingService : IWeddingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WeddingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<WeddingDto> CreateWeddingAsync(CreateWeddingDto weddingDto, int userId)
        {
            var wedding = new Wedding
            {
                WeddingName = weddingDto.WeddingName,
                WeddingDate = weddingDto.WeddingDate,
                TotalBudget = weddingDto.TotalBudget,
                OwnerUserId = userId
            };

            await _unitOfWork.Weddings.AddAsync(wedding);
            await _unitOfWork.CompleteAsync();

            var result = new WeddingDto
            {
                WeddingID = wedding.WeddingId,
                WeddingName = wedding.WeddingName,
                WeddingDate = wedding.WeddingDate,
                TotalBudget = wedding.TotalBudget,
                OwnerUserId = wedding.OwnerUserId
            };

            // Notify subscribers about the new wedding
            Console.WriteLine($"WeddingService: Notifying about Created wedding {wedding.WeddingId} for user {userId}");
            await NotifyWeddingChange(wedding, WeddingUpdateEvent.Types.UpdateType.Created);

            return result;
        }

        public async Task<bool> DeleteWeddingAsync(int weddingId, int? ownerUserId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);
            if (wedding == null)
            {
                return false;
            }

            // If ownerUserId is provided (not admin), verify ownership
            if (ownerUserId.HasValue && wedding.OwnerUserId != ownerUserId.Value)
            {
                return false;
            }

            // Store user ID and wedding details before deletion for notification
            var userId = wedding.OwnerUserId;
            var weddingDetails = CreateWeddingDetails(wedding);

            await _unitOfWork.Weddings.DeleteAsync(weddingId);
            await _unitOfWork.CompleteAsync();

            // Notify subscribers about the deleted wedding
            Console.WriteLine($"WeddingService: Notifying about Deleted wedding {weddingId} for user {userId}");
            await WeddingUpdateGrpcService.NotifyWeddingChange(
                userId,
                WeddingUpdateEvent.Types.UpdateType.Deleted,
                weddingDetails);

            return true;
        }

        public async Task<WeddingDto?> GetWeddingByIdAsync(int weddingId, int? ownerUserId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);
            if (wedding == null)
            {
                return null;
            }

            // If ownerUserId is provided (not admin), verify ownership
            if (ownerUserId.HasValue && wedding.OwnerUserId != ownerUserId.Value)
            {
                return null;
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

        public async Task<IEnumerable<WeddingDto>> GetWeddingsAsync(int userId)
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

        public async Task<bool> UpdateWeddingAsync(int weddingId, CreateWeddingDto weddingDto, int? ownerUserId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);
            if (wedding == null)
            {
                return false;
            }

            // If ownerUserId is provided (not admin), verify ownership
            if (ownerUserId.HasValue && wedding.OwnerUserId != ownerUserId.Value)
            {
                return false;
            }

            wedding.WeddingName = weddingDto.WeddingName;
            wedding.WeddingDate = weddingDto.WeddingDate;
            wedding.TotalBudget = weddingDto.TotalBudget;

            await _unitOfWork.CompleteAsync();

            Console.WriteLine($"WeddingService: Notifying about Updated wedding {wedding.WeddingId} for user {wedding.OwnerUserId}");
            await NotifyWeddingChange(wedding, WeddingUpdateEvent.Types.UpdateType.Updated);

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

        private async System.Threading.Tasks.Task NotifyWeddingChange(Wedding wedding, WeddingUpdateEvent.Types.UpdateType updateType)
        {
            var weddingDetails = CreateWeddingDetails(wedding);

            await WeddingUpdateGrpcService.NotifyWeddingChange(
                wedding.OwnerUserId,
                updateType,
                weddingDetails);
        }

        private WeddingDetails CreateWeddingDetails(Wedding wedding)
        {
            return new WeddingDetails
            {
                WeddingId = wedding.WeddingId,
                WeddingName = wedding.WeddingName ?? string.Empty,
                WeddingDate = Timestamp.FromDateTime(wedding.WeddingDate.ToUniversalTime()),
                TotalBudget = (double)wedding.TotalBudget,
                OwnerUserId = wedding.OwnerUserId
            };
        }
    }
}