namespace TravelManagement.Domain.Entities;

public class Notification
{
    public int NotificationId { get; set; }

    // Foreign Key
    public int UserId { get; set; }

    // Notification Information
    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string NotificationType { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedDate { get; set; }

    // Navigation Property
    public User User { get; set; } = null!;
}