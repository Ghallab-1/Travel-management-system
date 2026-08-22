using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public HotelsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Hotels
                .Include(h => h.City)
                .Where(h => h.IsActive)
                .OrderBy(h => h.HotelName)
                .Select(h => new
                {
                    h.HotelId,
                    h.HotelName,
                    h.CityId,
                    cityName = h.City.CityName,
                    h.Rating,
                    h.Address,
                    h.Phone
                })
                .ToListAsync();

            return Ok(items);
        }
    }
}
