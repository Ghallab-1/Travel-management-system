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
    public class FlightsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public FlightsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await FlightQuery()
                .OrderByDescending(f => f.DepartureDateTime)
                .ThenBy(f => f.FlightId)
                .ToListAsync();

            return Ok(items.Select(ToDto));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await FlightQuery()
                .FirstOrDefaultAsync(f => f.FlightId == id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(ToDto(item));
        }

        [HttpPost]
        [Authorize(Policy = "CoordinatorOnly")]
        public async Task<IActionResult> Create([FromBody] CreateFlightDto input)
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
                return BadRequest("Flights can be booked only after Department Manager approval.");
            }

            var airline = await _db.Airlines.FindAsync(input.AirlineId);
            if (airline == null)
            {
                return BadRequest("Airline not found.");
            }

            var flight = new Flight
            {
                TravelBookingId = input.TravelBookingId,
                AirlineId = input.AirlineId,
                FlightNumber = input.FlightNumber,
                BookingReference = input.BookingReference,
                DepartureAirport = input.DepartureAirport,
                ArrivalAirport = input.ArrivalAirport,
                DepartureDateTime = input.DepartureDateTime,
                ArrivalDateTime = input.ArrivalDateTime
            };

            _db.Flights.Add(flight);
            AddNotification(
                booking.TravelRequest.UserId,
                "Flight booked",
                $"Flight {flight.FlightNumber} was booked for request #{booking.TravelRequestId}.",
                "Flight");

            await _db.SaveChangesAsync();

            var created = await FlightQuery()
                .FirstAsync(f => f.FlightId == flight.FlightId);

            return CreatedAtAction(nameof(Get), new { id = flight.FlightId }, ToDto(created));
        }

        private IQueryable<Flight> FlightQuery()
        {
            return _db.Flights
                .Include(f => f.Airline)
                .Include(f => f.TravelBooking)
                    .ThenInclude(b => b.TravelRequest)
                        .ThenInclude(r => r.User);
        }

        private static object ToDto(Flight flight)
        {
            return new
            {
                flight.FlightId,
                flight.TravelBookingId,
                travelRequestId = flight.TravelBooking?.TravelRequestId,
                requesterName = flight.TravelBooking?.TravelRequest?.User?.FullName,
                flight.AirlineId,
                airlineName = flight.Airline?.AirlineName,
                flight.FlightNumber,
                flight.BookingReference,
                flight.DepartureAirport,
                flight.ArrivalAirport,
                flight.DepartureDateTime,
                flight.ArrivalDateTime
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
