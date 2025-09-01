using BandBaaajaVivaah.Data.Models;
using Task = BandBaaajaVivaah.Data.Models.Task;

namespace BandBaaajaVivaah.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Wedding> Weddings { get; }
        IRepository<Guest> Guests { get; }
        IRepository<Expense> Expenses { get; }
        IRepository<Task> Tasks { get; }
        Task<int> CompleteAsync();
    }
}
