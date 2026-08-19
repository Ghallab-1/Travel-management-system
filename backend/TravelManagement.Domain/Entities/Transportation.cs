namespace TravelManagement.Domain.Entities;

public class Transportation
{
    public int TransportationId { get; set; }

    // Foreign Key
    public int TravelBookingId { get; set; }

    // Transportation Information
    public string TransportationType { get; set; } = string.Empty;

    public string ProviderName { get; set; } = string.Empty;

    public string DriverName { get; set; } = string.Empty;

    public string DriverPhone { get; set; } = string.Empty;

    public string PickupLocation { get; set; } = string.Empty;

    public string DropOffLocation { get; set; } = string.Empty;

    public DateTime PickupDateTime { get; set; }

    // Navigation Property
    public TravelBooking TravelBooking { get; set; } = null!;
}