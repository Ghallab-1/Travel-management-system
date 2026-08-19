namespace TravelManagement.Domain.Entities;

public class City
{
    public int CityId { get; set; }

    // Foreign Key
    public int CountryId { get; set; }

    public string CityName { get; set; } = string.Empty;

    // Navigation Properties
    public Country Country { get; set; } = null!;

    public ICollection<TravelRequest> TravelRequests { get; set; } = new List<TravelRequest>();

    public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
}