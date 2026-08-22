using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitiesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CitiesController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Cities
                .Include(c => c.Country)
                .OrderBy(c => c.Country.CountryName)
                .ThenBy(c => c.CityName)
                .Select(c => new { id = c.CityId, name = c.CityName, country = c.Country.CountryName })
                .ToListAsync();
            return Ok(items);
        }
    }
}
