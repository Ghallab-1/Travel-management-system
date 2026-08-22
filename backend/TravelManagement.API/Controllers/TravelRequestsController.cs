using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    public class TravelRequestsController : ControllerBase
    {
        private const int FinalApprovalLevel = 3;
        private const string DirectManagerRole = "Direct Manager";
        private const string DepartmentManagerRole = "Department Manager";
        private const string TravelCoordinatorRole = "Travel Coordinator";
        private const string HrRole = "HR";

        private readonly AppDbContext _db;

        public TravelRequestsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await GetApprovalRolesAsync();

            var items = await BaseRequestQuery()
                .OrderBy(t => t.TravelRequestId)
                .ToListAsync();

            return Ok(items.Select(item => ToRequestDto(item, roles)));
        }

        [HttpGet("pending-for-me")]
        [Authorize]
        public async Task<IActionResult> GetPendingForMe()
        {
            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return Unauthorized("Could not determine the logged-in user.");
            }

            if (currentUser.Role == null)
            {
                return Unauthorized("Logged-in user does not have a role.");
            }

            if (currentUser.Role.Level > FinalApprovalLevel)
            {
                return Ok(Array.Empty<object>());
            }

            var roles = await GetApprovalRolesAsync();

            var items = await BaseRequestQuery()
                .Where(r =>
                    r.CurrentApprovalLevel == currentUser.Role.Level &&
                    r.Status.ToLower() == "pending")
                .OrderBy(r => r.TravelRequestId)
                .ToListAsync();

            return Ok(items.Select(item => ToRequestDto(item, roles)));
        }

        [HttpGet("coordinator-work")]
        [Authorize(Policy = "CoordinatorOnly")]
        public async Task<IActionResult> GetCoordinatorWork()
        {
            var roles = await GetApprovalRolesAsync();

            var items = await BaseRequestQuery()
                .Include(t => t.TravelApprovals)
                    .ThenInclude(a => a.Approver)
                        .ThenInclude(u => u.Role)
                .OrderByDescending(t => t.CreatedDate)
                .ThenBy(t => t.TravelRequestId)
                .ToListAsync();

            return Ok(items.Select(item => ToRequestDto(item, roles, includeApprovals: true)));
        }

        [HttpGet("hr-review")]
        [Authorize(Policy = "HrOnly")]
        public async Task<IActionResult> GetHrReview()
        {
            var roles = await GetApprovalRolesAsync();

            var items = await BaseRequestQuery()
                .Include(t => t.TravelApprovals)
                    .ThenInclude(a => a.Approver)
                        .ThenInclude(u => u.Role)
                .OrderByDescending(t => t.CreatedDate)
                .ThenBy(t => t.TravelRequestId)
                .ToListAsync();

            return Ok(items.Select(item => ToRequestDto(item, roles, includeApprovals: true)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var roles = await GetApprovalRolesAsync();

            var item = await BaseRequestQuery()
                .Include(t => t.TravelApprovals)
                    .ThenInclude(a => a.Approver)
                        .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(t => t.TravelRequestId == id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(ToRequestDto(item, roles, includeApprovals: true));
        }

        [HttpGet("{id:int}/document")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var item = await _db.TravelRequests
                .FirstOrDefaultAsync(t => t.TravelRequestId == id);

            if (item == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(item.RequiredDocumentFileBase64))
            {
                return NotFound("No required document PDF is attached to this request.");
            }

            try
            {
                var bytes = Convert.FromBase64String(item.RequiredDocumentFileBase64);
                var contentType = item.RequiredDocumentFileContentType ?? "application/pdf";
                var fileName = item.RequiredDocumentFileName ?? $"travel-request-{id}-documents.pdf";

                return File(bytes, contentType, fileName);
            }
            catch (FormatException)
            {
                return BadRequest("The stored PDF payload is invalid.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTravelRequestDto input)
        {
            if (input == null)
            {
                return BadRequest();
            }

            var requester = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == input.UserId);

            if (requester == null)
            {
                return BadRequest("User not found.");
            }

            if (requester.Role == null)
            {
                return BadRequest("User role not found.");
            }

            if (!IsValidPdfMetadata(input.RequiredDocumentFileName, input.RequiredDocumentFileContentType))
            {
                return BadRequest("Only PDF attachments are supported for travel documents.");
            }

            var nextApprovalLevel = requester.Role.Level + 1;
            if (nextApprovalLevel > FinalApprovalLevel)
            {
                nextApprovalLevel = FinalApprovalLevel;
            }

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
                EstimatedBudget = 0m,
                Status = "Pending",
                CurrentApprovalLevel = nextApprovalLevel,
                CreatedDate = DateTime.UtcNow,
                RequiredDocumentNotes = TrimOrNull(input.RequiredDocumentNotes),
                RequiredDocumentFileName = TrimOrNull(input.RequiredDocumentFileName),
                RequiredDocumentFileContentType = TrimOrNull(input.RequiredDocumentFileContentType),
                RequiredDocumentFileBase64 = TrimOrNull(input.RequiredDocumentFileBase64),
                PerDiemStatus = "Not Submitted"
            };

            _db.TravelRequests.Add(request);
            await _db.SaveChangesAsync();

            var roles = await GetApprovalRolesAsync();
            var nextRoleName = roles.GetValueOrDefault(request.CurrentApprovalLevel, DepartmentManagerRole);

            AddNotification(
                request.UserId,
                "Travel request submitted",
                $"Your request #{request.TravelRequestId} is waiting for {nextRoleName}.",
                "TravelRequest");

            await AddNotificationsForRoleAsync(
                nextRoleName,
                "Travel request waiting for approval",
                $"{requester.FullName} submitted request #{request.TravelRequestId}.",
                "Approval");

            await _db.SaveChangesAsync();

            var created = await LoadRequestForResponseAsync(request.TravelRequestId);

            return CreatedAtAction(
                nameof(Get),
                new { id = request.TravelRequestId },
                ToRequestDto(created!, roles, includeApprovals: true));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TravelRequest input)
        {
            if (input == null || id != input.TravelRequestId)
            {
                return BadRequest();
            }

            var exists = await _db.TravelRequests.AnyAsync(t => t.TravelRequestId == id);
            if (!exists)
            {
                return NotFound();
            }

            _db.Entry(input).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id:int}/coordinator-details")]
        [Authorize(Policy = "CoordinatorOnly")]
        public async Task<IActionResult> UpdateCoordinatorDetails(
            int id,
            [FromBody] UpdateCoordinatorDetailsDto input)
        {
            if (input == null)
            {
                return BadRequest();
            }

            if (input.EstimatedBudget < 0)
            {
                return BadRequest("Estimated budget cannot be negative.");
            }

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Could not determine the logged-in user.");
            }

            if (input.CoordinatorId != 0 && input.CoordinatorId != currentUser.Id)
            {
                return BadRequest("Coordinator id must match the logged-in user.");
            }

            var request = await _db.TravelRequests
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TravelRequestId == id);

            if (request == null)
            {
                return NotFound("TravelRequest not found.");
            }

            if (!IsApproved(request))
            {
                return BadRequest("Managers must approve the request before travel coordination can begin.");
            }

            request.EstimatedBudget = input.EstimatedBudget;
            request.EstimatedBudgetSetById = currentUser.Id;
            request.EstimatedBudgetSetDate = DateTime.UtcNow;
            request.CoordinatorNotes = TrimOrNull(input.CoordinatorNotes);

            AddNotification(
                request.UserId,
                "Travel budget updated",
                $"Travel coordinator updated the estimated budget for request #{request.TravelRequestId}.",
                "Coordinator");

            await AddNotificationsForRoleAsync(
                HrRole,
                "Travel budget ready for HR review",
                $"Request #{request.TravelRequestId} has an estimated budget for per diem review.",
                "PerDiem");

            await _db.SaveChangesAsync();

            var roles = await GetApprovalRolesAsync();
            var item = await LoadRequestForResponseAsync(id);

            return Ok(ToRequestDto(item!, roles, includeApprovals: true));
        }

        [HttpPatch("{id:int}/per-diem")]
        [Authorize(Policy = "HrOnly")]
        public async Task<IActionResult> UpdatePerDiem(int id, [FromBody] UpdatePerDiemDto input)
        {
            if (input == null)
            {
                return BadRequest();
            }

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Could not determine the logged-in user.");
            }

            if (input.HrUserId != 0 && input.HrUserId != currentUser.Id)
            {
                return BadRequest("HR user id must match the logged-in user.");
            }

            var decision = NormalizeDecision(input.Decision);
            if (decision == null)
            {
                return BadRequest("Per diem decision must be Approved or Rejected.");
            }

            if (decision == "Approved" && input.PerDiemAmount <= 0)
            {
                return BadRequest("Approved per diem amount must be greater than zero.");
            }

            var request = await _db.TravelRequests
                .Include(t => t.User)
                    .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(t => t.TravelRequestId == id);

            if (request == null)
            {
                return NotFound("TravelRequest not found.");
            }

            if (!IsApproved(request))
            {
                return BadRequest("HR can review per diem only after manager approval is complete.");
            }

            request.PerDiemAmount = decision == "Approved" ? input.PerDiemAmount : 0m;
            request.PerDiemStatus = decision;
            request.PerDiemApprovedById = currentUser.Id;
            request.PerDiemComments = TrimOrNull(input.Comments);
            request.PerDiemDecisionDate = DateTime.UtcNow;

            AddNotification(
                request.UserId,
                "Per diem review completed",
                $"HR {decision.ToLower()} per diem for request #{request.TravelRequestId}.",
                "PerDiem");

            await AddNotificationsForRoleAsync(
                TravelCoordinatorRole,
                "Per diem review completed",
                $"HR {decision.ToLower()} per diem for request #{request.TravelRequestId}.",
                "PerDiem");

            await _db.SaveChangesAsync();

            var roles = await GetApprovalRolesAsync();
            var item = await LoadRequestForResponseAsync(id);

            return Ok(ToRequestDto(item!, roles, includeApprovals: true));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.TravelRequests.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

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
            {
                return BadRequest();
            }

            if (id != input.TravelRequestId)
            {
                return BadRequest("Mismatched travel request id.");
            }

            var request = await _db.TravelRequests
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TravelRequestId == id);

            if (request == null)
            {
                return NotFound("TravelRequest not found.");
            }

            if (!IsPending(request))
            {
                return BadRequest("Only pending travel requests can be approved.");
            }

            var approver = await GetCurrentUserAsync();
            if (approver == null)
            {
                return Unauthorized("Could not determine the logged-in user.");
            }

            if (input.ApproverId != 0 && input.ApproverId != approver.Id)
            {
                return BadRequest("Approver id must match the logged-in user.");
            }

            if (approver.Role == null)
            {
                return BadRequest("Approver role not found.");
            }

            if (approver.Role.Level != request.CurrentApprovalLevel)
            {
                return BadRequest(
                    $"This request needs approval from {await GetRoleNameAsync(request.CurrentApprovalLevel)}, " +
                    $"but {approver.Role.RoleName} is level {approver.Role.Level}.");
            }

            var approval = new TravelApproval
            {
                TravelRequestId = input.TravelRequestId,
                ApproverId = approver.Id,
                ApprovalLevel = approver.Role.RoleName,
                Decision = "Approved",
                Comments = TrimOrNull(input.Comments),
                ActionDate = DateTime.UtcNow
            };

            _db.TravelApprovals.Add(approval);

            if (request.CurrentApprovalLevel >= FinalApprovalLevel)
            {
                request.Status = "Approved";
                request.CurrentApprovalLevel = FinalApprovalLevel;
                request.PerDiemStatus = "Pending";

                AddNotification(
                    request.UserId,
                    "Travel request approved",
                    $"Your request #{request.TravelRequestId} was approved by {DepartmentManagerRole}.",
                    "Approval");

                await AddNotificationsForRoleAsync(
                    TravelCoordinatorRole,
                    "Travel request ready for booking",
                    $"Request #{request.TravelRequestId} is approved and ready for booking.",
                    "Coordinator");

                await AddNotificationsForRoleAsync(
                    HrRole,
                    "Travel request ready for per diem review",
                    $"Request #{request.TravelRequestId} is approved and ready for per diem review.",
                    "PerDiem");
            }
            else
            {
                request.CurrentApprovalLevel += 1;
                request.Status = "Pending";

                var roles = await GetApprovalRolesAsync();
                var nextRole = roles.GetValueOrDefault(request.CurrentApprovalLevel, DepartmentManagerRole);

                AddNotification(
                    request.UserId,
                    "Travel request approval progressed",
                    $"Your request #{request.TravelRequestId} is now waiting for {nextRole}.",
                    "Approval");

                await AddNotificationsForRoleAsync(
                    nextRole,
                    "Travel request waiting for approval",
                    $"Request #{request.TravelRequestId} is waiting for {nextRole}.",
                    "Approval");
            }

            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = request.TravelRequestId },
                ToApprovalDto(approval));
        }

        [HttpPost("{id:int}/reject")]
        [Authorize(Policy = "ApproverOnly")]
        public async Task<IActionResult> Reject(
            int id,
            [FromBody] CreateTravelApprovalDto input)
        {
            if (input == null)
            {
                return BadRequest();
            }

            if (id != input.TravelRequestId)
            {
                return BadRequest("Mismatched travel request id.");
            }

            var request = await _db.TravelRequests
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TravelRequestId == id);

            if (request == null)
            {
                return NotFound("TravelRequest not found.");
            }

            if (!IsPending(request))
            {
                return BadRequest("Only pending travel requests can be rejected.");
            }

            var approver = await GetCurrentUserAsync();
            if (approver == null)
            {
                return Unauthorized("Could not determine the logged-in user.");
            }

            if (input.ApproverId != 0 && input.ApproverId != approver.Id)
            {
                return BadRequest("Approver id must match the logged-in user.");
            }

            if (approver.Role == null)
            {
                return BadRequest("Approver role not found.");
            }

            if (approver.Role.Level != request.CurrentApprovalLevel)
            {
                return BadRequest(
                    $"This request needs action from {await GetRoleNameAsync(request.CurrentApprovalLevel)}, " +
                    $"but {approver.Role.RoleName} is level {approver.Role.Level}.");
            }

            var approval = new TravelApproval
            {
                TravelRequestId = input.TravelRequestId,
                ApproverId = approver.Id,
                ApprovalLevel = approver.Role.RoleName,
                Decision = "Rejected",
                Comments = TrimOrNull(input.Comments),
                ActionDate = DateTime.UtcNow
            };

            _db.TravelApprovals.Add(approval);

            request.Status = "Rejected";
            request.PerDiemStatus = "Not Submitted";

            AddNotification(
                request.UserId,
                "Travel request rejected",
                $"Your request #{request.TravelRequestId} was rejected by {approver.Role.RoleName}.",
                "Approval");

            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = request.TravelRequestId },
                ToApprovalDto(approval));
        }

        private IQueryable<TravelRequest> BaseRequestQuery()
        {
            return _db.TravelRequests
                .Include(t => t.User)
                    .ThenInclude(u => u.Role)
                .Include(t => t.DestinationCity)
                    .ThenInclude(c => c.Country)
                .Include(t => t.EstimatedBudgetSetBy)
                    .ThenInclude(u => u!.Role)
                .Include(t => t.PerDiemApprovedBy)
                    .ThenInclude(u => u!.Role);
        }

        private async Task<TravelRequest?> LoadRequestForResponseAsync(int id)
        {
            return await BaseRequestQuery()
                .Include(t => t.TravelApprovals)
                    .ThenInclude(a => a.Approver)
                        .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(t => t.TravelRequestId == id);
        }

        private async Task<Dictionary<int, string>> GetApprovalRolesAsync()
        {
            return await _db.Roles
                .Where(r => r.Level >= 1 && r.Level <= FinalApprovalLevel)
                .ToDictionaryAsync(r => r.Level, r => r.RoleName);
        }

        private async Task<string> GetRoleNameAsync(int level)
        {
            return await _db.Roles
                .Where(r => r.Level == level)
                .Select(r => r.RoleName)
                .FirstOrDefaultAsync() ?? $"level {level}";
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var claim =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User.FindFirstValue("sub");

            if (!int.TryParse(claim, out var userId))
            {
                return null;
            }

            return await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        private object ToRequestDto(
            TravelRequest item,
            IReadOnlyDictionary<int, string> roles,
            bool includeApprovals = false)
        {
            var currentApprovalRole =
                roles.GetValueOrDefault(item.CurrentApprovalLevel) ??
                (IsApproved(item) ? DepartmentManagerRole : null);

            var approvals = includeApprovals
                ? item.TravelApprovals
                    .OrderBy(a => a.ActionDate)
                    .Select(a => new
                    {
                        a.TravelApprovalId,
                        a.ApproverId,
                        approverName = a.Approver?.FullName,
                        approverRole = a.Approver?.Role?.RoleName,
                        a.ApprovalLevel,
                        a.Decision,
                        comments = TrimOrNull(a.Comments),
                        a.ActionDate
                    })
                    .ToList()
                : null;

            return new
            {
                item.TravelRequestId,
                item.UserId,
                userName = item.User?.FullName,
                userRole = item.User?.Role?.RoleName,
                item.DepartmentId,
                item.TravelPolicyId,
                item.DestinationCityId,
                destinationCityName = item.DestinationCity?.CityName,
                destinationCountryName = item.DestinationCity?.Country?.CountryName,
                item.Purpose,
                item.Project,
                item.TravelType,
                item.DepartureDate,
                item.ReturnDate,
                item.EstimatedBudget,
                item.EstimatedBudgetSetById,
                estimatedBudgetSetByName = item.EstimatedBudgetSetBy?.FullName,
                item.EstimatedBudgetSetDate,
                item.Status,
                item.CurrentApprovalLevel,
                currentApprovalRole,
                item.CreatedDate,
                item.RequiredDocumentNotes,
                item.RequiredDocumentFileName,
                item.RequiredDocumentFileContentType,
                hasRequiredDocumentPdf = !string.IsNullOrWhiteSpace(item.RequiredDocumentFileBase64),
                item.CoordinatorNotes,
                item.PerDiemAmount,
                item.PerDiemStatus,
                item.PerDiemApprovedById,
                perDiemApprovedByName = item.PerDiemApprovedBy?.FullName,
                item.PerDiemComments,
                item.PerDiemDecisionDate,
                approvals
            };
        }

        private static object ToApprovalDto(TravelApproval approval)
        {
            return new
            {
                approval.TravelApprovalId,
                approval.TravelRequestId,
                approval.ApproverId,
                approval.ApprovalLevel,
                approval.Decision,
                Comments = TrimOrNull(approval.Comments),
                approval.ActionDate
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

        private async Task AddNotificationsForRoleAsync(
            string roleName,
            string title,
            string message,
            string type)
        {
            var userIds = await _db.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive && u.Role.RoleName == roleName)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var userId in userIds)
            {
                AddNotification(userId, title, message, type);
            }
        }

        private static bool IsPending(TravelRequest request)
        {
            return string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsApproved(TravelRequest request)
        {
            return string.Equals(request.Status, "Approved", StringComparison.OrdinalIgnoreCase);
        }

        private static string? TrimOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string? NormalizeDecision(string? value)
        {
            if (string.Equals(value, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                return "Approved";
            }

            if (string.Equals(value, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                return "Rejected";
            }

            return null;
        }

        private static bool IsValidPdfMetadata(string? fileName, string? contentType)
        {
            if (string.IsNullOrWhiteSpace(fileName) && string.IsNullOrWhiteSpace(contentType))
            {
                return true;
            }

            return string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
                   (!string.IsNullOrWhiteSpace(fileName) &&
                    fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
        }
    }
}
