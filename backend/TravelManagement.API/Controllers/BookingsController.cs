using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;
using TravelManagement.Domain.Entities;
using TravelManagement.API.Models;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public BookingsController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.TravelBookings.ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _db.TravelBookings.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("byrequest/{requestId:int}")]
        public async Task<IActionResult> GetByRequest(int requestId)
        {
            var items = await _db.TravelBookings.Where(tb => tb.TravelRequestId == requestId).ToListAsync();
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTravelBookingDto input)
        {
            if (input == null) return BadRequest();

            var request = await _db.TravelRequests.FindAsync(input.TravelRequestId);
            if (request == null) return BadRequest("TravelRequest not found");

            var booking = new TravelBooking
            {
                TravelRequestId = input.TravelRequestId,
                BookingStatus = input.BookingStatus,
                BookingDate = DateOnly.FromDateTime(DateTime.Today),
                Notes = input.Notes
            };

            _db.TravelBookings.Add(booking);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = booking.TravelBookingId }, booking);
        }
    }
}
