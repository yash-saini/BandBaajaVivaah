using BandBaaajaVivaah.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Task = BandBaaajaVivaah.Data.Models.Task;

namespace BandBaaajaVivaah.Data.Repositories
{
    public interface ITaskRepository : IRepository<Task>
    {
        Task<IEnumerable<Task>> FindAllAsync(Expression<Func<Task, bool>> predicate);
    }

    public class TaskRepository : Repository<Task>, ITaskRepository
    {
        public TaskRepository(BandBaajaVivaahDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Task>> FindAllAsync(Expression<Func<Task, bool>> predicate)
        {
            return await _context.Tasks.Where(predicate).ToListAsync();
        }
    }
}
