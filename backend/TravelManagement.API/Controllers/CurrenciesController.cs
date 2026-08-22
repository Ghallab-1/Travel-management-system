using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrenciesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CurrenciesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Currencies
                .OrderBy(c => c.CurrencyCode)
                .Select(c => new
                {
                    c.CurrencyId,
                    c.CurrencyCode,
                    c.CurrencyName,
                    c.ExchangeRate
                })
                .ToListAsync();

            return Ok(items);
        }
    }
}
