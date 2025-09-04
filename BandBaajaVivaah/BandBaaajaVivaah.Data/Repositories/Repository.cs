using BandBaaajaVivaah.Data.Models;
using Task = System.Threading.Tasks.Task;

namespace BandBaaajaVivaah.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly BandBaajaVivaahDbContext _context;

        public Repository(BandBaajaVivaahDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Task.FromResult(_context.Set<T>().ToList());
        }
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }
        public Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            return System.Threading.Tasks.Task.CompletedTask;
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
            }
        }
    }
}
