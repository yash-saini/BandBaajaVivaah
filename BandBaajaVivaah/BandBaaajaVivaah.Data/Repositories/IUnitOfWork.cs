using BandBaaajaVivaah.Data.Models;

namespace BandBaaajaVivaah.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IWeddingRepository Weddings { get; }
        IGuestRepository Guests { get; }
        IExpenseRepository Expenses { get; }
        ITaskRepository Tasks { get; }
        Task<int> CompleteAsync();
    }
}
