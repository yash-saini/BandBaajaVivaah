using BandBaaajaVivaah.Data.Models;
using System.Linq.Expressions;

namespace BandBaaajaVivaah.Data.Repositories
{
    public interface IGuestRepository : IRepository<Guest>
    {
        Task<IEnumerable<Guest>> FindAllAsync(Expression<Func<Guest, bool>> predicate);
    }

    public class GuestRepository : Repository<Guest>, IGuestRepository
    {
        public GuestRepository(BandBaajaVivaahDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Guest>> FindAllAsync(Expression<Func<Guest, bool>> predicate)
        {
            return await System.Threading.Tasks.Task.FromResult(_context.Guests.Where(predicate).ToList());
        }
    }
}
