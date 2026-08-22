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
    public class HotelReservationsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public HotelReservationsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await ReservationQuery()
                .OrderByDescending(h => h.CheckInDate)
                .ThenBy(h => h.HotelReservationId)
                .ToListAsync();

            return Ok(items.Select(ToDto));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await ReservationQuery()
                .FirstOrDefaultAsync(h => h.HotelReservationId == id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(ToDto(item));
        }

        [HttpPost]
        [Authorize(Policy = "CoordinatorOnly")]
        public async Task<IActionResult> Create([FromBody] CreateHotelReservationDto input)
        {
            if (input == null)
            {
                return BadRequest();
            }

            var booking = await _db.TravelBookings
                .Include(b => b.TravelRequest)
                .FirstOrDefaultAsync(b => b.TravelBookingId == input.TravelBookingId);

            if (booking == null)
            {
                return BadRequest("TravelBooking not found.");
            }

            if (!IsApproved(booking.TravelRequest))
            {
                return BadRequest("Hotel reservations can be created only after Department Manager approval.");
            }

            var hotel = await _db.Hotels.FindAsync(input.HotelId);
            if (hotel == null)
            {
                return BadRequest("Hotel not found.");
            }

            var reservation = new HotelReservation
            {
                TravelBookingId = input.TravelBookingId,
                HotelId = input.HotelId,
                CheckInDate = input.CheckInDate,
                CheckOutDate = input.CheckOutDate,
                NumberOfNights = input.NumberOfNights,
                BookingReference = input.BookingReference
            };

            _db.HotelReservations.Add(reservation);
            AddNotification(
                booking.TravelRequest.UserId,
                "Hotel booked",
                $"A hotel reservation was booked for request #{booking.TravelRequestId}.",
                "Hotel");

            await _db.SaveChangesAsync();

            var created = await ReservationQuery()
                .FirstAsync(h => h.HotelReservationId == reservation.HotelReservationId);

            return CreatedAtAction(nameof(Get), new { id = reservation.HotelReservationId }, ToDto(created));
        }

        private IQueryable<HotelReservation> ReservationQuery()
        {
            return _db.HotelReservations
                .Include(h => h.Hotel)
                    .ThenInclude(h => h.City)
                .Include(h => h.TravelBooking)
                    .ThenInclude(b => b.TravelRequest)
                        .ThenInclude(r => r.User);
        }

        private static object ToDto(HotelReservation reservation)
        {
            return new
            {
                reservation.HotelReservationId,
                reservation.TravelBookingId,
                travelRequestId = reservation.TravelBooking?.TravelRequestId,
                requesterName = reservation.TravelBooking?.TravelRequest?.User?.FullName,
                reservation.HotelId,
                hotelName = reservation.Hotel?.HotelName,
                cityName = reservation.Hotel?.City?.CityName,
                reservation.CheckInDate,
                reservation.CheckOutDate,
                reservation.NumberOfNights,
                reservation.BookingReference
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
