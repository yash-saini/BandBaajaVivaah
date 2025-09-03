using BandBaaajaVivaah.Data.Models;
using BandBaaajaVivaah.Data.Repositories;

namespace BandBaajaVivaah.Services
{
    public interface IWeddingService
    {
        Task<Wedding?> GetWeddingByIdAsync(int weddingId);
        Task<Wedding> CreateWeddingAsync(Wedding newWedding);
    }

    public class WeddingService : IWeddingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WeddingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Wedding?> GetWeddingByIdAsync(int weddingId)
        {
            return await _unitOfWork.Weddings.GetByIdAsync(weddingId);
        }

        public async Task<Wedding> CreateWeddingAsync(Wedding newWedding)
        {
            await _unitOfWork.Weddings.AddAsync(newWedding);
            await _unitOfWork.CompleteAsync();
            return newWedding;
        }
    }
}