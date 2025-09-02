using BandBaaajaVivaah.Data.Models;
using Task = BandBaaajaVivaah.Data.Models.Task;

namespace BandBaaajaVivaah.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BandBaajaVivaahDbContext _context;
        public IRepository<User> Users { get; private set; }
        public IRepository<Wedding> Weddings { get; private set; }
        public IRepository<Guest> Guests { get; private set; }
        public IRepository<Expense> Expenses { get; private set; }
        public IRepository<Task> Tasks { get; private set; }
        public UnitOfWork(BandBaajaVivaahDbContext context)
        {
            _context = context;
            Users = new Repository<User>(context);
            Weddings = new Repository<Wedding>(context);
            Guests = new Repository<Guest>(context);
            Expenses = new Repository<Expense>(context);
            Tasks = new Repository<Task>(context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
