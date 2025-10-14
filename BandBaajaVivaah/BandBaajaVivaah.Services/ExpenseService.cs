using BandBaaajaVivaah.Data.Models;
using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.Grpc;
using BandBaajaVivaah.Services.GrpcServices;

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

            // Check if user is admin
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            bool isAdmin = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            if (wedding == null || (!isAdmin && wedding.OwnerUserId != userId))
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

            var result = new ExpenseDto
            {
                ExpenseID = expense.ExpenseId,
                Description = expense.Description,
                Amount = expense.Amount,
                Category = expense.Category,
                PaymentDate = expense.PaymentDate
            };

            // Notify subscribers about the new expense
            Console.WriteLine($"ExpenseService: Notifying about Created expense {expense.ExpenseId} for wedding {expense.WeddingId}");
            await NotifyExpenseChange(expense, ExpenseUpdateEvent.Types.UpdateType.Created);

            return result;
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

            // Check if user is admin
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            bool isAdmin = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            if (wedding == null || (!isAdmin && wedding.OwnerUserId != userId))
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

            // Notify subscribers about the updated expense
            Console.WriteLine($"ExpenseService: Notifying about Updated expense {expense.ExpenseId} for wedding {expense.WeddingId}");
            await NotifyExpenseChange(expense, ExpenseUpdateEvent.Types.UpdateType.Updated);

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

            // Check if user is admin
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            bool isAdmin = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            if (wedding == null || (!isAdmin && wedding.OwnerUserId != userId))
            {
                return false;
            }

            return await DeleteExpensesAsAdminAsync(expenseId);
        }

        public async Task<bool> DeleteExpensesAsAdminAsync(int expenseId)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId);
            if (expense == null) return false;

            // Store wedding ID and expense details before deletion for notification
            var weddingId = expense.WeddingId;
            var expenseDetails = CreateExpenseDetails(expense);

            await _unitOfWork.Expenses.DeleteAsync(expenseId);
            await _unitOfWork.CompleteAsync();

            // Notify subscribers about the deleted expense
            Console.WriteLine($"ExpenseService: Notifying about Deleted expense {expenseId} for wedding {weddingId}");
            await ExpenseUpdateGrpcService.NotifyExpenseChange(
                weddingId,
                ExpenseUpdateEvent.Types.UpdateType.Deleted,
                expenseDetails);

            return true;
        }

        private async System.Threading.Tasks.Task NotifyExpenseChange(Expense expense, ExpenseUpdateEvent.Types.UpdateType updateType)
        {
            var expenseDetails = CreateExpenseDetails(expense);

            await ExpenseUpdateGrpcService.NotifyExpenseChange(
                expense.WeddingId,
                updateType,
                expenseDetails);
        }

        // Helper to create gRPC ExpenseDetails object from Expense model
        private ExpenseDetails CreateExpenseDetails(Expense expense)
        {
            return new ExpenseDetails
            {
                ExpenseId = expense.ExpenseId,
                Description = expense.Description ?? string.Empty,
                Amount = (double)expense.Amount,
                Category = expense.Category ?? string.Empty,
                WeddingId = expense.WeddingId
            };
        }
    }
}
