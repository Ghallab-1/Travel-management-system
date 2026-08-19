using Microsoft.EntityFrameworkCore;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Department> Departments => Set<Department>();

        public DbSet<Country> Countries => Set<Country>();
        public DbSet<City> Cities => Set<City>();

        public DbSet<TravelPolicy> TravelPolicies => Set<TravelPolicy>();
        public DbSet<TravelRequest> TravelRequests => Set<TravelRequest>();
        public DbSet<TravelApproval> TravelApprovals => Set<TravelApproval>();

        public DbSet<TravelBooking> TravelBookings => Set<TravelBooking>();
        public DbSet<Airline> Airlines => Set<Airline>();
        public DbSet<Flight> Flights => Set<Flight>();
        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<HotelReservation> HotelReservations => Set<HotelReservation>();
        public DbSet<Transportation> Transportations => Set<Transportation>();
        public DbSet<Visa> Visas => Set<Visa>();
        public DbSet<Insurance> Insurances => Set<Insurance>();

        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ExpenseReceipt> ExpenseReceipts => Set<ExpenseReceipt>();

        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply IEntityTypeConfiguration classes from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}