using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;
using TravelManagement.Domain.Entities;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public NotificationsController(AppDbContext db) => _db = db;

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            var items = await _db.Notifications.Where(n => n.UserId == userId).ToListAsync();
            return Ok(items);
        }

        [HttpPost("markread/{id:int}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var item = await _db.Notifications.FindAsync(id);
            if (item == null) return NotFound();
            item.IsRead = true;
            _db.Entry(item).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
