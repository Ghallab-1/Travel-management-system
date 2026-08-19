using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;
using TravelManagement.Domain.Entities;
using TravelManagement.API.Models;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelReservationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HotelReservationsController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.HotelReservations.ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _db.HotelReservations.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHotelReservationDto input)
        {
            if (input == null) return BadRequest();

            var booking = await _db.TravelBookings.FindAsync(input.TravelBookingId);
            if (booking == null) return BadRequest("TravelBooking not found");

            var hotel = await _db.Hotels.FindAsync(input.HotelId);
            if (hotel == null) return BadRequest("Hotel not found");

            var reservation = new HotelReservation
            {
                TravelBookingId = input.TravelBookingId,
                HotelId = input.HotelId,
                CheckInDate = input.CheckInDate,
                CheckOutDate = input.CheckOutDate,
                NumberOfNights = input.NumberOfNights,
                BookingReference = input.BookingReference,
            };

            _db.HotelReservations.Add(reservation);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = reservation.HotelReservationId }, reservation);
        }
    }
}
