using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;
using TravelManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using TravelManagement.API.Models;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TravelRequestsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TravelRequestsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.TravelRequests.ToListAsync();
            return Ok(items);
        }

        [HttpGet("pending-for-me")]
        public async Task<IActionResult> GetPendingForMe()
        {
            // Lightweight server-side endpoint that returns pending requests.
            // For now this returns requests that are not Approved or Rejected.
            var items = await _db.TravelRequests
                .Where(r => r.Status == null || (r.Status.ToLower() != "approved" && r.Status.ToLower() != "rejected"))
                .ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _db.TravelRequests.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TravelRequest input)
        {
            if (input == null) return BadRequest();
            input.CreatedDate = DateTime.UtcNow;
            _db.TravelRequests.Add(input);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = input.TravelRequestId }, input);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TravelRequest input)
        {
            if (input == null || id != input.TravelRequestId) return BadRequest();
            var exists = await _db.TravelRequests.AnyAsync(t => t.TravelRequestId == id);
            if (!exists) return NotFound();
            _db.Entry(input).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.TravelRequests.FindAsync(id);
            if (item == null) return NotFound();
            _db.TravelRequests.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Inline approve endpoint to match simpler patterns: creates an approval record and updates request status
        [HttpPost("{id:int}/approve")]
        [Authorize(Policy = "ApproverOnly")]
        public async Task<IActionResult> Approve(int id, [FromBody] CreateTravelApprovalDto input)
        {
            if (input == null) return BadRequest();
            if (id != input.TravelRequestId) return BadRequest("Mismatched travel request id");

            var request = await _db.TravelRequests.FindAsync(id);
            if (request == null) return NotFound("TravelRequest not found");

            var approver = await _db.Users.FindAsync(input.ApproverId);
            if (approver == null) return BadRequest("Approver not found");

            var approval = new TravelApproval
            {
                TravelRequestId = input.TravelRequestId,
                ApproverId = input.ApproverId,
                ApprovalLevel = input.ApprovalLevel,
                Decision = "Approved",
                Comments = input.Comments,
                ActionDate = DateTime.UtcNow
            };

            _db.TravelApprovals.Add(approval);
            request.Status = "Approved";
            _db.Entry(request).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = approval.TravelApprovalId }, approval);
        }

        [HttpPost("{id:int}/reject")]
        [Authorize(Policy = "ApproverOnly")]
        public async Task<IActionResult> Reject(int id, [FromBody] CreateTravelApprovalDto input)
        {
            if (input == null) return BadRequest();
            if (id != input.TravelRequestId) return BadRequest("Mismatched travel request id");

            var request = await _db.TravelRequests.FindAsync(id);
            if (request == null) return NotFound("TravelRequest not found");

            var approver = await _db.Users.FindAsync(input.ApproverId);
            if (approver == null) return BadRequest("Approver not found");

            var approval = new TravelApproval
            {
                TravelRequestId = input.TravelRequestId,
                ApproverId = input.ApproverId,
                ApprovalLevel = input.ApprovalLevel,
                Decision = "Rejected",
                Comments = input.Comments,
                ActionDate = DateTime.UtcNow
            };

            _db.TravelApprovals.Add(approval);
            request.Status = "Rejected";
            _db.Entry(request).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = approval.TravelApprovalId }, approval);
        }
    }
}
