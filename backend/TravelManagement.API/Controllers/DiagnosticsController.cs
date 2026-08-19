using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public DiagnosticsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Only allow diagnostics in Development to avoid exposing internals in production
            if (!_env.IsDevelopment())
            {
                return Forbid();
            }

            var usersCount = await _db.Users.CountAsync();
            var rolesCount = await _db.Roles.CountAsync();
            var requestsCount = await _db.TravelRequests.CountAsync();
            var approvalsCount = await _db.TravelApprovals.CountAsync();
            var demoUserExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == "demo@company.com");

            return Ok(new
            {
                Environment = _env.EnvironmentName,
                Users = usersCount,
                Roles = rolesCount,
                TravelRequests = requestsCount,
                TravelApprovals = approvalsCount,
                DemoUserExists = demoUserExists
            });
        }
    }
}
