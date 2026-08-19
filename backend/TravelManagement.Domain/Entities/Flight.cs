namespace TravelManagement.Domain.Entities;

public class Flight
{
    public int FlightId { get; set; }

    // Foreign Keys
    public int TravelBookingId { get; set; }

    public int AirlineId { get; set; }

    // Flight Information
    public string FlightNumber { get; set; } = string.Empty;

    public string BookingReference { get; set; } = string.Empty;

    public string DepartureAirport { get; set; } = string.Empty;

    public string ArrivalAirport { get; set; } = string.Empty;

    public DateTime DepartureDateTime { get; set; }

    public DateTime ArrivalDateTime { get; set; }

    // Navigation Properties
    public TravelBooking TravelBooking { get; set; } = null!;

    public Airline Airline { get; set; } = null!;
}