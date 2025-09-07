using BandBaaajaVivaah.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BandBaaajaVivaah.Data.Repositories
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<IEnumerable<Expense>> FindAllAsync(Expression<Func<Expense, bool>> predicate);
    }

    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(BandBaajaVivaahDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Expense>> FindAllAsync(Expression<Func<Expense, bool>> predicate)
        {
            return await _context.Expenses.Where(predicate).ToListAsync();
        }
    }
}
