namespace TravelManagement.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Department Department { get; set; } = null!;
        public Role Role { get; set; } = null!;

        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<TravelApproval> ApprovalsGiven { get; set; } = new List<TravelApproval>();
        public ICollection<TravelRequest> TravelRequests { get; set; } = new List<TravelRequest>();

        // Authentication
        public string PasswordHash { get; set; } = string.Empty;
    }
}