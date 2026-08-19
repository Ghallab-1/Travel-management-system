using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;
using TravelManagement.Domain.Entities;
using TravelManagement.API.Models;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ExpensesController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Expenses.ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _db.Expenses.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("travelrequest/{requestId:int}")]
        public async Task<IActionResult> GetByTravelRequest(int requestId)
        {
            var items = await _db.Expenses.Where(e => e.TravelRequestId == requestId).ToListAsync();
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpenseDto input)
        {
            if (input == null) return BadRequest();

            var request = await _db.TravelRequests.FindAsync(input.TravelRequestId);
            if (request == null) return BadRequest("TravelRequest not found");

            var user = await _db.Users.FindAsync(input.UserId);
            if (user == null) return BadRequest("User not found");

            var category = await _db.ExpenseCategories.FindAsync(input.ExpenseCategoryId);
            if (category == null) return BadRequest("ExpenseCategory not found");

            var currency = await _db.Currencies.FindAsync(input.CurrencyId);
            if (currency == null) return BadRequest("Currency not found");

            var expense = new Expense
            {
                TravelRequestId = input.TravelRequestId,
                UserId = input.UserId,
                ExpenseCategoryId = input.ExpenseCategoryId,
                CurrencyId = input.CurrencyId,
                Amount = input.Amount,
                Description = input.Description,
                ExpenseDate = input.ExpenseDate,
                Status = "Submitted",
                SubmittedDate = DateTime.UtcNow
            };

            _db.Expenses.Add(expense);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = expense.ExpenseId }, expense);
        }
    }
}
