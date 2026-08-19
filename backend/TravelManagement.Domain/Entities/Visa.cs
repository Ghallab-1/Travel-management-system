namespace TravelManagement.Domain.Entities;

public class Visa
{
    public int VisaId { get; set; }

    // Foreign Keys
    public int TravelBookingId { get; set; }

    public int CountryId { get; set; }

    // Visa Information
    public string VisaType { get; set; } = string.Empty;

    public DateOnly ApplicationDate { get; set; }

    public DateOnly? ApprovalDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public string Status { get; set; } = string.Empty;

    // Navigation Properties
    public TravelBooking TravelBooking { get; set; } = null!;

    public Country Country { get; set; } = null!;
}