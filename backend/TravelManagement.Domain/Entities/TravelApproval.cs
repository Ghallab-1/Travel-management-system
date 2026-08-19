namespace TravelManagement.Domain.Entities;

public class TravelApproval
{
    public int TravelApprovalId { get; set; }

    // Foreign Keys
    public int TravelRequestId { get; set; }

    public int ApproverId { get; set; }

    // Approval Information
    public string ApprovalLevel { get; set; } = string.Empty;

    public string Decision { get; set; } = string.Empty;

    public string? Comments { get; set; }

    public DateTime ActionDate { get; set; }

    // Navigation Properties
    public TravelRequest TravelRequest { get; set; } = null!;

    public User Approver { get; set; } = null!;
}