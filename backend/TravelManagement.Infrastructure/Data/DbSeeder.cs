using Microsoft.EntityFrameworkCore;
using TravelManagement.Domain.Entities;

namespace TravelManagement.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            // Seed roles (idempotent)
            var desiredRoles = new[]
            {
                "Employee",
                "Direct Manager",
                "Department Manager",
                "HR",
                "Finance",
                "Travel Coordinator",
                "Admin"
            };

            foreach (var rn in desiredRoles)
            {
                if (!db.Roles.Any(r => r.RoleName == rn))
                {
                    db.Roles.Add(new Role { RoleName = rn, IsActive = true });
                }
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

            // Ensure demo user exists (idempotent)
            var demoEmail = "demo@company.com";
            if (!db.Users.Any(u => u.Email.ToLower() == demoEmail))
            {
                var directManager = db.Roles.FirstOrDefault(r => r.RoleName == "Direct Manager") ?? db.Roles.First();
                var department = db.Departments.FirstOrDefault() ?? new Department { DepartmentName = "Engineering", IsActive = true };

                // If department was just created in-memory and not tracked, ensure it's saved
                if (department.Id == 0)
                {
                    db.Departments.Add(department);
                    db.SaveChanges();
                }

                var demo = new User
                {
                    FullName = "Demo Manager",
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




