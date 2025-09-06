using BandBaaajaVivaah.Data.Models;

namespace BandBaaajaVivaah.Data.Repositories
{
    public interface IWeddingRepository : IRepository<Wedding>
    {
        Task<IEnumerable<Wedding?>> GetAllForUserAsync(int userId);
    }

    public class WeddingRepository : Repository<Wedding>, IWeddingRepository
    {
        public WeddingRepository(BandBaajaVivaahDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Wedding?>> GetAllForUserAsync(int userId)
        {
            return await System.Threading.Tasks.Task.FromResult(_context.Weddings.Where(w => w.OwnerUserId == userId).ToList());
        }
    }
}
