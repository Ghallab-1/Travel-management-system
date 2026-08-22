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
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BookingsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await BookingQuery()
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.TravelBookingId)
                .ToListAsync();

            return Ok(items.Select(ToDto));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await BookingQuery()
                .FirstOrDefaultAsync(b => b.TravelBookingId == id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(ToDto(item));
        }

        [HttpGet("byrequest/{requestId:int}")]
        public async Task<IActionResult> GetByRequest(int requestId)
        {
            var items = await BookingQuery()
                .Where(tb => tb.TravelRequestId == requestId)
                .OrderByDescending(tb => tb.BookingDate)
                .ToListAsync();

            return Ok(items.Select(ToDto));
        }

        [HttpPost]
        [Authorize(Policy = "CoordinatorOnly")]
        public async Task<IActionResult> Create([FromBody] CreateTravelBookingDto input)
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
                return BadRequest("Bookings can be created only after Department Manager approval.");
            }

            var booking = new TravelBooking
            {
                TravelRequestId = input.TravelRequestId,
                BookingStatus = string.IsNullOrWhiteSpace(input.BookingStatus)
                    ? "Pending"
                    : input.BookingStatus.Trim(),
                BookingDate = DateOnly.FromDateTime(DateTime.Today),
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim()
            };

            _db.TravelBookings.Add(booking);
            AddNotification(
                request.UserId,
                "Travel booking created",
                $"A travel booking was created for request #{request.TravelRequestId}.",
                "Booking");

            await _db.SaveChangesAsync();

            var created = await BookingQuery()
                .FirstAsync(b => b.TravelBookingId == booking.TravelBookingId);

            return CreatedAtAction(nameof(Get), new { id = booking.TravelBookingId }, ToDto(created));
        }

        private IQueryable<TravelBooking> BookingQuery()
        {
            return _db.TravelBookings
                .Include(b => b.TravelRequest)
                    .ThenInclude(r => r.User)
                .Include(b => b.TravelRequest)
                    .ThenInclude(r => r.DestinationCity);
        }

        private static object ToDto(TravelBooking booking)
        {
            return new
            {
                booking.TravelBookingId,
                booking.TravelRequestId,
                requesterName = booking.TravelRequest?.User?.FullName,
                destinationCityName = booking.TravelRequest?.DestinationCity?.CityName,
                booking.BookingStatus,
                booking.BookingDate,
                booking.Notes
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
