using BandBaaajaVivaah.Data.Models;
using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTO;

namespace BandBaajaVivaah.Services
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDto>> GetExpensesByWeddingIdAsync(int weddingId, int userId);
        Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto expenseDto, int userId);
        Task<bool> UpdateExpenseAsync(int expenseId, CreateExpenseDto expenseDto, int userId);
        Task<bool> DeleteExpenseAsync(int expenseId, int userId);

        Task<ExpenseDto> CreateExpensesAsAdminAsync(CreateExpenseDto expenseDto);
        Task<bool> UpdateExpensesAsAdminAsync(int expenseId, CreateExpenseDto expenseDto);
        Task<bool> DeleteExpensesAsAdminAsync(int expenseId);
    }

    public class ExpenseService : IExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExpenseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto expenseDto, int userId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(expenseDto.WeddingID);
            if (wedding == null || wedding.OwnerUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to add an expense to this wedding.");
            }
            return await CreateExpensesAsAdminAsync(expenseDto);
        }

        public async Task<ExpenseDto> CreateExpensesAsAdminAsync(CreateExpenseDto expenseDto)
        {
            var expense = new Expense
            {
                Description = expenseDto.Description,
                Amount = expenseDto.Amount,
                Category = expenseDto.Category,
                PaymentDate = expenseDto.PaymentDate,
                WeddingId = expenseDto.WeddingID,
            };
            await _unitOfWork.Expenses.AddAsync(expense);
            await _unitOfWork.CompleteAsync();
            return new ExpenseDto
            {
                ExpenseID = expense.ExpenseId,
                Description = expense.Description,
                Amount = expense.Amount,
                Category = expense.Category,
                PaymentDate = expense.PaymentDate
            };
        }

        public async Task<IEnumerable<ExpenseDto>> GetExpensesByWeddingIdAsync(int weddingId, int userId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            bool isAdmin = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;

            // Deny access if the wedding doesn't exist, OR if the user is not an admin AND doesn't own the wedding
            if (wedding == null || (!isAdmin && wedding.OwnerUserId != userId))
            {
                throw new UnauthorizedAccessException("You are not authorized to view expenses for this wedding.");
            }

            var expenses = await _unitOfWork.Expenses.FindAllAsync(e => e.WeddingId == weddingId);
            return expenses.Select(e => new ExpenseDto
            {
                ExpenseID = e.ExpenseId,
                Description = e.Description,
                Amount = e.Amount,
                Category = e.Category,
                PaymentDate = e.PaymentDate
            });
        }

        public async Task<bool> UpdateExpenseAsync(int expenseId, CreateExpenseDto expenseDto, int userId)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return false;
            }
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(expense.WeddingId);
            if (wedding == null || wedding.OwnerUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this expense.");
            }
            return await UpdateExpensesAsAdminAsync(expenseId, expenseDto);
        }

        public async Task<bool> UpdateExpensesAsAdminAsync(int expenseId, CreateExpenseDto expenseDto)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return false;
            }
            expense.Description = expenseDto.Description;
            expense.Amount = expenseDto.Amount;
            expense.Category = expenseDto.Category;
            expense.PaymentDate = expenseDto.PaymentDate;
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteExpenseAsync(int expenseId, int userId)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId);
            if (expense == null)
            {
                return false;
            }
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(expense.WeddingId);
            if (wedding == null || wedding.OwnerUserId != userId)
            {
                return false;
            }
            return await DeleteExpensesAsAdminAsync(expenseId);
        }

        public async Task<bool> DeleteExpensesAsAdminAsync(int expenseId)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId);
            if (expense == null) return false;
            await _unitOfWork.Expenses.DeleteAsync(expenseId);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
