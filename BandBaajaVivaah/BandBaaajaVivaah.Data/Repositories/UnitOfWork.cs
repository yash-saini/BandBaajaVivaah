using BandBaaajaVivaah.Data.Models;

namespace BandBaaajaVivaah.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BandBaajaVivaahDbContext _context;
        public IUserRepository Users  { get; private set; }
        public IWeddingRepository Weddings { get; private set; }
        public IGuestRepository Guests { get; private set; }
        public IExpenseRepository Expenses { get; private set; }
        public ITaskRepository Tasks { get; private set; }
        public UnitOfWork(BandBaajaVivaahDbContext context)
        {
            _context = context;
            Users = new UserRepository(context);
            Weddings = new WeddingRepository(context);
            Guests = new GuestRepository(context);
            Expenses = new ExpenseRepository(context);
            Tasks = new TaskRepository(context);
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
