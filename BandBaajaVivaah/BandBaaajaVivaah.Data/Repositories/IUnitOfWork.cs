using BandBaaajaVivaah.Data.Models;

namespace BandBaaajaVivaah.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IWeddingRepository Weddings { get; }
        IRepository<Guest> Guests { get; }
        IRepository<Expense> Expenses { get; }
        ITaskRepository Tasks { get; }
        Task<int> CompleteAsync();
    }
}
