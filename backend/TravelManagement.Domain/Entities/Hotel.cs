namespace TravelManagement.Domain.Entities;

public class Hotel
{
    public int HotelId { get; set; }

    // Foreign Key
    public int CityId { get; set; }

    // Hotel Information
    public string HotelName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Phone { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public City City { get; set; } = null!;

    public ICollection<HotelReservation> HotelReservations { get; set; } = new List<HotelReservation>();
}