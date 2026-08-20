namespace TravelManagement.Domain.Entities;

public class TravelRequest
{
    public int TravelRequestId { get; set; }

    // Foreign Keys
    public int UserId { get; set; }

    public int DepartmentId { get; set; }

    public int TravelPolicyId { get; set; }

    public int DestinationCityId { get; set; }

    // Travel Information
    public string Purpose { get; set; } = string.Empty;

    public string Project { get; set; } = string.Empty;

    public string TravelType { get; set; } = string.Empty;

    public DateOnly DepartureDate { get; set; }

    public DateOnly ReturnDate { get; set; }

    public decimal EstimatedBudget { get; set; }

    public string Status { get; set; } = string.Empty;

    public int CurrentApprovalLevel { get; set; }    // ADD THIS LINE — the Role.Level required to approve next

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;

    public Department Department { get; set; } = null!;

    public TravelPolicy TravelPolicy { get; set; } = null!;

    public City DestinationCity { get; set; } = null!;

    public ICollection<TravelApproval> TravelApprovals { get; set; } = new List<TravelApproval>();

    public ICollection<TravelBooking> TravelBookings { get; set; } = new List<TravelBooking>();

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}