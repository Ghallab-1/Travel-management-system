using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.API.Models;
using TravelManagement.Domain.Entities;
using TravelManagement.Infrastructure.Data;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ExpensesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await ExpenseQuery()
                .OrderByDescending(e => e.ExpenseDate)
                .ThenBy(e => e.ExpenseId)
                .ToListAsync();

            return Ok(items.Select(ToDto));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await ExpenseQuery()
                .FirstOrDefaultAsync(e => e.ExpenseId == id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(ToDto(item));
        }

        [HttpGet("travelrequest/{requestId:int}")]
        public async Task<IActionResult> GetByTravelRequest(int requestId)
        {
            var items = await ExpenseQuery()
                .Where(e => e.TravelRequestId == requestId)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();

            return Ok(items.Select(ToDto));
        }

        [HttpPost]
        [Authorize(Policy = "CoordinatorOnly")]
        public async Task<IActionResult> Create([FromBody] CreateExpenseDto input)
        {
            if (input == null)
            {
                return BadRequest();
            }

            var request = await _db.TravelRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.TravelRequestId == input.TravelRequestId);

            if (request == null)
            {
                return BadRequest("TravelRequest not found.");
            }

            if (!IsApproved(request))
            {
                return BadRequest("Expenses can be added only after Department Manager approval.");
            }

            var user = await _db.Users.FindAsync(input.UserId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var category = await _db.ExpenseCategories.FindAsync(input.ExpenseCategoryId);
            if (category == null)
            {
                return BadRequest("ExpenseCategory not found.");
            }

            var currency = await _db.Currencies.FindAsync(input.CurrencyId);
            if (currency == null)
            {
                return BadRequest("Currency not found.");
            }

            var expense = new Expense
            {
                TravelRequestId = input.TravelRequestId,
                UserId = input.UserId,
                ExpenseCategoryId = input.ExpenseCategoryId,
                CurrencyId = input.CurrencyId,
                Amount = input.Amount,
                Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
                ExpenseDate = input.ExpenseDate,
                Status = "Submitted",
                SubmittedDate = DateTime.UtcNow
            };

            _db.Expenses.Add(expense);
            AddNotification(
                request.UserId,
                "Travel expense recorded",
                $"An expense of {expense.Amount:0.00} was added to request #{request.TravelRequestId}.",
                "Expense");

            await _db.SaveChangesAsync();

            var created = await ExpenseQuery()
                .FirstAsync(e => e.ExpenseId == expense.ExpenseId);

            return CreatedAtAction(nameof(Get), new { id = expense.ExpenseId }, ToDto(created));
        }

        private IQueryable<Expense> ExpenseQuery()
        {
            return _db.Expenses
                .Include(e => e.User)
                    .ThenInclude(u => u.Role)
                .Include(e => e.TravelRequest)
                    .ThenInclude(r => r.User)
                .Include(e => e.ExpenseCategory)
                .Include(e => e.Currency);
        }

        private static object ToDto(Expense expense)
        {
            return new
            {
                expense.ExpenseId,
                expense.TravelRequestId,
                requesterName = expense.TravelRequest?.User?.FullName,
                expense.UserId,
                userName = expense.User?.FullName,
                userRole = expense.User?.Role?.RoleName,
                expense.ExpenseCategoryId,
                categoryName = expense.ExpenseCategory?.CategoryName,
                expense.CurrencyId,
                currencyCode = expense.Currency?.CurrencyCode,
                currencyName = expense.Currency?.CurrencyName,
                expense.Amount,
                expense.Description,
                expense.ExpenseDate,
                expense.Status,
                expense.SubmittedDate
            };
        }

        private void AddNotification(int userId, string title, string message, string type)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = type,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });
        }

        private static bool IsApproved(TravelRequest request)
        {
            return string.Equals(request.Status, "Approved", StringComparison.OrdinalIgnoreCase);
        }
    }
}
