using BandBaaajaVivaah.Data.Models;
using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTO;

namespace BandBaajaVivaah.Services
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDto>> GetExpensesByWeddingIdAsync(int weddingId, int userId);
        Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto expenseDto, int userId);
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

            var expense = new Expense
            {
                Description = expenseDto.Description,
                Amount = expenseDto.Amount,
                Category = expenseDto.Category,
                PaymentDate = expenseDto.PaymentDate,
                WeddingId = expenseDto.WeddingID
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
            if (wedding == null || wedding.OwnerUserId != userId)
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
    }
}
