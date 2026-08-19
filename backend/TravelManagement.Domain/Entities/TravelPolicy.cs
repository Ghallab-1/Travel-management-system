namespace TravelManagement.Domain.Entities;

public class TravelPolicy
{
    public int TravelPolicyId { get; set; }

    public string PolicyName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string TravelType { get; set; } = string.Empty;

    public decimal MaximumBudget { get; set; }

    public decimal MaximumHotelAllowance { get; set; }

    public decimal MaximumMealAllowance { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Property
    public ICollection<TravelRequest> TravelRequests { get; set; } = new List<TravelRequest>();
}