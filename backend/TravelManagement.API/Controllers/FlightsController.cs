using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;
using TravelManagement.Domain.Entities;
using TravelManagement.API.Models;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public FlightsController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Flights.ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _db.Flights.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFlightDto input)
        {
            if (input == null) return BadRequest();

            var booking = await _db.TravelBookings.FindAsync(input.TravelBookingId);
            if (booking == null) return BadRequest("TravelBooking not found");

            var airline = await _db.Airlines.FindAsync(input.AirlineId);
            if (airline == null) return BadRequest("Airline not found");

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
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = flight.FlightId }, flight);
        }
    }
}
