namespace TravelManagement.Domain.Entities;

public class TravelBooking
{
    public int TravelBookingId { get; set; }

    public int TravelRequestId { get; set; }

    public string BookingStatus { get; set; } = string.Empty;

    public DateOnly BookingDate { get; set; }

    public string? Notes { get; set; }

    public TravelRequest TravelRequest { get; set; } = null!;

    public ICollection<Flight> Flights { get; set; } = new List<Flight>();

    public ICollection<HotelReservation> HotelReservations { get; set; } = new List<HotelReservation>();

    public ICollection<Transportation> Transportations { get; set; } = new List<Transportation>();

    public ICollection<Visa> Visas { get; set; } = new List<Visa>();

    public ICollection<Insurance> Insurances { get; set; } = new List<Insurance>();
}