using Microsoft.EntityFrameworkCore;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            // Seed roles with hierarchy levels (idempotent)
            var desiredRoles = new (string Name, int Level)[]
            {
                ("Employee", 1),
                ("Direct Manager", 2),
                ("Department Manager", 3),
                ("Travel Coordinator", 4),
                ("HR", 0),
                ("Finance", 0),
                ("Admin", 99)
            };

            foreach (var (name, level) in desiredRoles)
            {
                var existing = db.Roles.FirstOrDefault(r => r.RoleName == name);
                if (existing == null)
                    db.Roles.Add(new Role { RoleName = name, Level = level, IsActive = true });
                else
                    existing.Level = level;
            }

            // Seed departments (idempotent)
            var desiredDepartments = new[] { "Engineering", "Sales" };
            foreach (var dn in desiredDepartments)
            {
                if (!db.Departments.Any(d => d.DepartmentName == dn))
                {
                    db.Departments.Add(new Department { DepartmentName = dn, IsActive = true });
                }
            }

            db.SaveChanges();

            // Seed countries and cities (idempotent) - required for TravelRequest.DestinationCityId
            if (!db.Countries.Any())
            {
                var usa = new Country { CountryName = "United States", CountryCode = "US" };
                var uk = new Country { CountryName = "United Kingdom", CountryCode = "GB" };
                var uae = new Country { CountryName = "United Arab Emirates", CountryCode = "AE" };
                db.Countries.AddRange(usa, uk, uae);
                db.SaveChanges();

                db.Cities.AddRange(
                    new City { CityName = "New York", CountryId = usa.CountryId },
                    new City { CityName = "San Francisco", CountryId = usa.CountryId },
                    new City { CityName = "London", CountryId = uk.CountryId },
                    new City { CityName = "Dubai", CountryId = uae.CountryId }
                );
                db.SaveChanges();
            }

            // Ensure demo user exists (idempotent) - this is your "Direct Manager"
            var demoEmail = "directmanager@company.com";
            if (!db.Users.Any(u => u.Email.ToLower() == demoEmail))
            {
                var directManager = db.Roles.FirstOrDefault(r => r.RoleName == "Direct Manager") ?? db.Roles.First();
                var department = db.Departments.FirstOrDefault() ?? new Department { DepartmentName = "Engineering", IsActive = true };

                if (department.Id == 0)
                {
                    db.Departments.Add(department);
                    db.SaveChanges();
                }

                var demo = new User
                {
                    FullName = "Direct Manager",
                    Email = demoEmail,
                    EmployeeNumber = "E1000",
                    DepartmentId = department.Id,
                    RoleId = directManager.Id,
                    IsActive = true,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
                };

                db.Users.Add(demo);
                db.SaveChanges();
            }

            // Ensure one user per hierarchy level for testing (idempotent)
            var defaultDept = db.Departments.First();

            void EnsureUser(string fullName, string email, string empNo, string roleName)
            {
                if (db.Users.Any(u => u.Email == email)) return;
                var role = db.Roles.First(r => r.RoleName == roleName);
                db.Users.Add(new User
                {
                    FullName = fullName,
                    Email = email,
                    EmployeeNumber = empNo,
                    DepartmentId = defaultDept.Id,
                    RoleId = role.Id,
                    IsActive = true,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
                });
            }

            EnsureUser("Employee One", "employee@company.com", "E1001", "Employee");
            EnsureUser("Department Manager", "deptmanager@company.com", "E1002", "Department Manager");
            EnsureUser("Travel Coordinator", "coordinator@company.com", "E1003", "Travel Coordinator");
            db.SaveChanges();

            // Seed lookup data: Currencies, ExpenseCategories, Airlines, TravelPolicies (idempotent)
            if (!db.Currencies.Any())
            {
                var currencies = new[]
                {
                    new Currency { CurrencyCode = "USD", CurrencyName = "US Dollar", ExchangeRate = 1m, LastUpdated = DateTime.UtcNow },
                    new Currency { CurrencyCode = "EUR", CurrencyName = "Euro", ExchangeRate = 0.92m, LastUpdated = DateTime.UtcNow },
                    new Currency { CurrencyCode = "GBP", CurrencyName = "British Pound", ExchangeRate = 0.78m, LastUpdated = DateTime.UtcNow }
                };
                db.Currencies.AddRange(currencies);
                db.SaveChanges();
            }

            if (!db.ExpenseCategories.Any())
            {
                var categories = new[]
                {
                    new ExpenseCategory { CategoryName = "Meals", Description = "Meals and refreshments", IsActive = true },
                    new ExpenseCategory { CategoryName = "Lodging", Description = "Hotel and accommodation", IsActive = true },
                    new ExpenseCategory { CategoryName = "Transportation", Description = "Ground transportation", IsActive = true }
                };
                db.ExpenseCategories.AddRange(categories);
                db.SaveChanges();
            }

            if (!db.Airlines.Any())
            {
                var airlines = new[]
                {
                    new Airline { AirlineName = "ExampleAir", AirlineCode = "EA", IsActive = true },
                    new Airline { AirlineName = "GlobalAir", AirlineCode = "GA", IsActive = true }
                };
                db.Airlines.AddRange(airlines);
                db.SaveChanges();
            }

            if (!db.TravelPolicies.Any())
            {
                var policies = new[]
                {
                    new TravelPolicy { PolicyName = "Default Policy", Description = "Default travel policy", TravelType = "Business", MaximumBudget = 5000m, MaximumHotelAllowance = 200m, MaximumMealAllowance = 50m, IsActive = true }
                };
                db.TravelPolicies.AddRange(policies);
                db.SaveChanges();
            }
        }
    }
}