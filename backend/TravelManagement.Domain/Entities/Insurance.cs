namespace TravelManagement.Domain.Entities;

public class Insurance
{
    public int InsuranceId { get; set; }

    // Foreign Key
    public int TravelBookingId { get; set; }

    // Insurance Information
    public string ProviderName { get; set; } = string.Empty;

    public string PolicyNumber { get; set; } = string.Empty;

    public string CoverageDetails { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Status { get; set; } = string.Empty;

    // Navigation Property
    public TravelBooking TravelBooking { get; set; } = null!;
}