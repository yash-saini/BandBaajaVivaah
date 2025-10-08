using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BandBaajaVivaah.Api.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new InvalidOperationException("User ID claim is missing or invalid.");
            }
            return int.Parse(userIdClaim);
        }

        [HttpGet("wedding/{weddingId}")]
        public async Task<IActionResult> GetExpensesForWedding(int weddingId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var expenses = await _expenseService.GetExpensesByWeddingIdAsync(weddingId, userId);
                return Ok(expenses);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto expenseDto)
        {
            try
            {
                if (User.IsInRole("Admin"))
                {
                    var createdExpense = await _expenseService.CreateExpensesAsAdminAsync(expenseDto);
                    return Ok(createdExpense);
                }
                else
                {
                    var userId = GetCurrentUserId();
                    var createdExpense = await _expenseService.CreateExpenseAsync(expenseDto, userId);
                    return Ok(createdExpense);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPut("{expenseId}")]
        public async Task<IActionResult> UpdateExpense(int expenseId, [FromBody] CreateExpenseDto expenseDto)
        {
            bool success;
            if (User.IsInRole("Admin"))
            {
                success = await _expenseService.UpdateExpensesAsAdminAsync(expenseId, expenseDto);
            }
            else
            {

               var userId = GetCurrentUserId();
               success = await _expenseService.UpdateExpenseAsync(expenseId, expenseDto, userId);
            }
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{expenseId}")]
        public async Task<IActionResult> DeleteExpense(int expenseId)
        {
            bool success;
            if (User.IsInRole("Admin"))
            {
                success = await _expenseService.DeleteExpensesAsAdminAsync(expenseId);
            }
            else
            {
                var userId = GetCurrentUserId();
                success = await _expenseService.DeleteExpenseAsync(expenseId, userId);
            }
            return success ? NoContent() : NotFound();
        }
    }
}