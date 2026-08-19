using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;
using TravelManagement.Domain.Entities;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DepartmentsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Departments
                .Select(d => new { id = d.Id, name = d.DepartmentName, description = d.Description, isActive = d.IsActive })
                .ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var d = await _db.Departments.FindAsync(id);
            if (d == null) return NotFound();
            return Ok(new { id = d.Id, name = d.DepartmentName, description = d.Description, isActive = d.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DepartmentCreateDto input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Name)) return BadRequest("Name is required");
            var dept = new Department { DepartmentName = input.Name.Trim(), Description = input.Description ?? string.Empty, IsActive = input.IsActive };
            _db.Departments.Add(dept);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = dept.Id }, new { id = dept.Id, name = dept.DepartmentName, description = dept.Description, isActive = dept.IsActive });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentCreateDto input)
        {
            var dept = await _db.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            dept.DepartmentName = input.Name ?? dept.DepartmentName;
            dept.Description = input.Description;
            dept.IsActive = input.IsActive;
            _db.Entry(dept).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dept = await _db.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            _db.Departments.Remove(dept);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        public class DepartmentCreateDto { public string? Name { get; set; } public string? Description { get; set; } public bool IsActive { get; set; } = true; }
    }
}
