namespace TravelManagement.Domain.Entities;

public class Airline
{
    public int AirlineId { get; set; }

    public string AirlineName { get; set; } = string.Empty;

    public string AirlineCode { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation Property
    public ICollection<Flight> Flights { get; set; } = new List<Flight>();
}