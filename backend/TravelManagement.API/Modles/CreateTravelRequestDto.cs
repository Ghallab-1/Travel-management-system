namespace TravelManagement.API.Models;

public class CreateTravelRequestDto
{
    public int UserId { get; set; }

    public int DepartmentId { get; set; }

    public int TravelPolicyId { get; set; }

    public int DestinationCityId { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public string Project { get; set; } = string.Empty;

    public string TravelType { get; set; } = string.Empty;

    public DateOnly DepartureDate { get; set; }

    public DateOnly ReturnDate { get; set; }

    public decimal EstimatedBudget { get; set; }

    public string? RequiredDocumentNotes { get; set; }

    public string? RequiredDocumentFileName { get; set; }

    public string? RequiredDocumentFileContentType { get; set; }

    public string? RequiredDocumentFileBase64 { get; set; }
}
