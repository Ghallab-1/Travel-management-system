using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;
using TravelManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using TravelManagement.API.Models;
using System.Security.Claims;

namespace TravelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TravelRequestsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TravelRequestsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.TravelRequests.ToListAsync();
            return Ok(items);
        }

        [HttpGet("pending-for-me")]
        [Authorize]
        public async Task<IActionResult> GetPendingForMe()
        {
            var roleName = User.FindFirstValue(ClaimTypes.Role);

            var role = await _db.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);

            if (role == null)
                return Ok(new List<TravelRequest>());

            var items = await _db.TravelRequests
                .Where(r =>
                    r.CurrentApprovalLevel == role.Level &&
                    r.Status != null &&
                    r.Status.ToLower() != "approved" &&
                    r.Status.ToLower() != "rejected")
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _db.TravelRequests
                .Include(t => t.DestinationCity)
                .Include(t => t.TravelApprovals)
                    .ThenInclude(a => a.Approver)
                .FirstOrDefaultAsync(t => t.TravelRequestId == id);

            if (item == null)
                return NotFound();

            var result = new
            {
                item.TravelRequestId,
                item.UserId,
                item.DepartmentId,
                item.TravelPolicyId,
                item.DestinationCityId,

                destinationCityName =
                    item.DestinationCity != null
                        ? item.DestinationCity.CityName
                        : null,

                item.Purpose,
                item.Project,
                item.TravelType,
                item.DepartureDate,
                item.ReturnDate,
                item.EstimatedBudget,
                item.Status,
                item.CurrentApprovalLevel,
                item.CreatedDate,

                approvals = item.TravelApprovals
                    .OrderBy(a => a.ActionDate)
                    .Select(a => new
                    {
                        a.TravelApprovalId,
                        a.ApproverId,
                        approverName =
                            a.Approver != null
                                ? a.Approver.FullName
                                : null,
                        a.ApprovalLevel,
                        a.Decision,
                        a.Comments,
                        a.ActionDate
                    })
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateTravelRequestDto input)
        {
            if (input == null)
                return BadRequest();

            var requester = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == input.UserId);

            if (requester == null)
                return BadRequest("User not found.");

            var request = new TravelRequest
            {
                UserId = input.UserId,
                DepartmentId = input.DepartmentId,
                TravelPolicyId = input.TravelPolicyId,
                DestinationCityId = input.DestinationCityId,

                Purpose = input.Purpose,
                Project = input.Project,
                TravelType = input.TravelType,
                DepartureDate = input.DepartureDate,
                ReturnDate = input.ReturnDate,
                EstimatedBudget = input.EstimatedBudget,

                Status = "Pending",
                CurrentApprovalLevel =
                    (requester.Role?.Level ?? 1) + 1,

                CreatedDate = DateTime.UtcNow
            };

            _db.TravelRequests.Add(request);

            await _db.SaveChangesAsync();

            var result = new
            {
                request.TravelRequestId,
                request.UserId,
                request.DepartmentId,
                request.TravelPolicyId,
                request.DestinationCityId,
                request.Purpose,
                request.Project,
                request.TravelType,
                request.DepartureDate,
                request.ReturnDate,
                request.EstimatedBudget,
                request.Status,
                request.CurrentApprovalLevel,
                request.CreatedDate
            };

            return CreatedAtAction(
                nameof(Get),
                new { id = request.TravelRequestId },
                result
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] TravelRequest input)
        {
            if (input == null || id != input.TravelRequestId)
                return BadRequest();

            var exists = await _db.TravelRequests
                .AnyAsync(t => t.TravelRequestId == id);

            if (!exists)
                return NotFound();

            _db.Entry(input).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.TravelRequests.FindAsync(id);

            if (item == null)
                return NotFound();

            _db.TravelRequests.Remove(item);

            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id:int}/approve")]
        [Authorize(Policy = "ApproverOnly")]
        public async Task<IActionResult> Approve(
            int id,
            [FromBody] CreateTravelApprovalDto input)
        {
            if (input == null)
                return BadRequest();

            if (id != input.TravelRequestId)
                return BadRequest("Mismatched travel request id");

            var request = await _db.TravelRequests
                .FindAsync(id);

            if (request == null)
                return NotFound("TravelRequest not found");

            var approver = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == input.ApproverId);

            if (approver == null)
                return BadRequest("Approver not found");

            if (approver.Role.Level != request.CurrentApprovalLevel)
            {
                return BadRequest(
                    $"This request needs approval from level " +
                    $"{request.CurrentApprovalLevel}, but " +
                    $"{approver.Role.RoleName} is level " +
                    $"{approver.Role.Level}");
            }

            var approval = new TravelApproval
            {
                TravelRequestId = input.TravelRequestId,
                ApproverId = input.ApproverId,
                ApprovalLevel = approver.Role.RoleName,
                Decision = "Approved",
                Comments = input.Comments,
                ActionDate = DateTime.UtcNow
            };

            _db.TravelApprovals.Add(approval);

            const int finalLevel = 4;

            if (request.CurrentApprovalLevel >= finalLevel)
            {
                request.Status = "Approved";
            }
            else
            {
                request.CurrentApprovalLevel += 1;
                request.Status = "Pending";
            }

            _db.Entry(request).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = request.TravelRequestId },
                new
                {
                    approval.TravelApprovalId,
                    approval.TravelRequestId,
                    approval.ApproverId,
                    approval.ApprovalLevel,
                    approval.Decision,
                    approval.Comments,
                    approval.ActionDate
                }
            );
        }

        [HttpPost("{id:int}/reject")]
        [Authorize(Policy = "ApproverOnly")]
        public async Task<IActionResult> Reject(
            int id,
            [FromBody] CreateTravelApprovalDto input)
        {
            if (input == null)
                return BadRequest();

            if (id != input.TravelRequestId)
                return BadRequest("Mismatched travel request id");

            var request = await _db.TravelRequests
                .FindAsync(id);

            if (request == null)
                return NotFound("TravelRequest not found");

            var approver = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == input.ApproverId);

            if (approver == null)
                return BadRequest("Approver not found");

            if (approver.Role.Level != request.CurrentApprovalLevel)
            {
                return BadRequest(
                    $"This request needs action from level " +
                    $"{request.CurrentApprovalLevel}, but " +
                    $"{approver.Role.RoleName} is level " +
                    $"{approver.Role.Level}");
            }

            var approval = new TravelApproval
            {
                TravelRequestId = input.TravelRequestId,
                ApproverId = input.ApproverId,
                ApprovalLevel = approver.Role.RoleName,
                Decision = "Rejected",
                Comments = input.Comments,
                ActionDate = DateTime.UtcNow
            };

            _db.TravelApprovals.Add(approval);

            request.Status = "Rejected";

            _db.Entry(request).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = request.TravelRequestId },
                new
                {
                    request.TravelRequestId,
                    request.UserId,
                    request.DepartmentId,
                    request.TravelPolicyId,
                    request.DestinationCityId,
                    request.Purpose,
                    request.Project,
                    request.TravelType,
                    request.DepartureDate,
                    request.ReturnDate,
                    request.EstimatedBudget,
                    request.Status,
                    request.CurrentApprovalLevel,
                    request.CreatedDate
                }
            );
        }
    }
}