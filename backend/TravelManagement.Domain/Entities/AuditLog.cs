namespace TravelManagement.Domain.Entities;

public class AuditLog
{
    public int AuditLogId { get; set; }

    // Foreign Key
    public int UserId { get; set; }

    // Audit Information
    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public int EntityId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime CreatedDate { get; set; }

    // Navigation Property
    public User User { get; set; } = null!;
}