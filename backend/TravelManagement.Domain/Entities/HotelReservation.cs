namespace TravelManagement.Domain.Entities;

public class HotelReservation
{
    public int HotelReservationId { get; set; }

    // Foreign Keys
    public int TravelBookingId { get; set; }

    public int HotelId { get; set; }

    // Reservation Information
    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public int NumberOfNights { get; set; }

    public string BookingReference { get; set; } = string.Empty;

    // Navigation Properties
    public TravelBooking TravelBooking { get; set; } = null!;

    public Hotel Hotel { get; set; } = null!;
}