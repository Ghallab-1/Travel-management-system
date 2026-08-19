namespace TravelManagement.Domain.Entities;

public class Country
{
    public int CountryId { get; set; }

    public string CountryName { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<City> Cities { get; set; } = new List<City>();

    public ICollection<Visa> Visas { get; set; } = new List<Visa>();
}