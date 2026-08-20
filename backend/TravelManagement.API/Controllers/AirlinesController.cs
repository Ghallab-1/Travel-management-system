using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;
using TravelManagement.Domain.Entities;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirlinesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AirlinesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var airlines = await _db.Airlines
                .OrderBy(a => a.AirlineName)
                .ToListAsync();

            return Ok(airlines);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var airline = await _db.Airlines
                .FirstOrDefaultAsync(a => a.AirlineId == id);

            if (airline == null)
                return NotFound();

            return Ok(airline);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Airline input)
        {
            if (input == null)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(input.AirlineName))
                return BadRequest("Airline name is required.");

            if (string.IsNullOrWhiteSpace(input.AirlineCode))
                return BadRequest("Airline code is required.");

            var codeExists = await _db.Airlines
                .AnyAsync(a => a.AirlineCode == input.AirlineCode);

            if (codeExists)
                return BadRequest("An airline with this code already exists.");

            var airline = new Airline
            {
                AirlineName = input.AirlineName.Trim(),
                AirlineCode = input.AirlineCode.Trim().ToUpper(),
                IsActive = input.IsActive
            };

            _db.Airlines.Add(airline);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = airline.AirlineId },
                airline
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] Airline input)
        {
            if (input == null)
                return BadRequest();

            var airline = await _db.Airlines
                .FirstOrDefaultAsync(a => a.AirlineId == id);

            if (airline == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(input.AirlineName))
                return BadRequest("Airline name is required.");

            if (string.IsNullOrWhiteSpace(input.AirlineCode))
                return BadRequest("Airline code is required.");

            var codeExists = await _db.Airlines
                .AnyAsync(a =>
                    a.AirlineId != id &&
                    a.AirlineCode == input.AirlineCode);

            if (codeExists)
                return BadRequest("An airline with this code already exists.");

            airline.AirlineName = input.AirlineName.Trim();
            airline.AirlineCode = input.AirlineCode.Trim().ToUpper();
            airline.IsActive = input.IsActive;

            await _db.SaveChangesAsync();

            return Ok(airline);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var airline = await _db.Airlines
                .FirstOrDefaultAsync(a => a.AirlineId == id);

            if (airline == null)
                return NotFound();

            var hasFlights = await _db.Flights
                .AnyAsync(f => f.AirlineId == id);

            if (hasFlights)
            {
                return BadRequest(
                    "This airline cannot be deleted because it has flights."
                );
            }

            _db.Airlines.Remove(airline);

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}