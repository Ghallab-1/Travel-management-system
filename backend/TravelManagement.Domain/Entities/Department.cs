namespace TravelManagement.Domain.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<TravelRequest> TravelRequests { get; set; } = new List<TravelRequest>();
    }
}