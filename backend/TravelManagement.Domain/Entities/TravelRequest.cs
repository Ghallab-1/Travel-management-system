namespace TravelManagement.Domain.Entities;

public class TravelRequest
{
    public int TravelRequestId { get; set; }

    public int UserId { get; set; }

    public int DepartmentId { get; set; }

    public int TravelPolicyId { get; set; }

    public int DestinationCityId { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public string Project { get; set; } = string.Empty;

    public string TravelType { get; set; } = string.Empty;

    public DateOnly DepartureDate { get; set; }

    public DateOnly ReturnDate { get; set; }

    public decimal EstimatedBudget { get; set; }

    public int? EstimatedBudgetSetById { get; set; }

    public DateTime? EstimatedBudgetSetDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public int CurrentApprovalLevel { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public string? RequiredDocumentNotes { get; set; }

    public string? RequiredDocumentFileName { get; set; }

    public string? RequiredDocumentFileContentType { get; set; }

    public string? RequiredDocumentFileBase64 { get; set; }

    public string? CoordinatorNotes { get; set; }

    public decimal? PerDiemAmount { get; set; }

    public string PerDiemStatus { get; set; } = "Not Submitted";

    public int? PerDiemApprovedById { get; set; }

    public string? PerDiemComments { get; set; }

    public DateTime? PerDiemDecisionDate { get; set; }

    public User User { get; set; } = null!;

    public Department Department { get; set; } = null!;

    public TravelPolicy TravelPolicy { get; set; } = null!;

    public City DestinationCity { get; set; } = null!;

    public User? EstimatedBudgetSetBy { get; set; }

    public User? PerDiemApprovedBy { get; set; }

    public ICollection<TravelApproval> TravelApprovals { get; set; } = new List<TravelApproval>();

    public ICollection<TravelBooking> TravelBookings { get; set; } = new List<TravelBooking>();

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
