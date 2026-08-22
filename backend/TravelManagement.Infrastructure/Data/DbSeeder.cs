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
                ("HR", 5),
                ("Finance", 6),
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

            Country EnsureCountry(string name, string code)
            {
                var country = db.Countries.FirstOrDefault(c => c.CountryCode == code);
                if (country != null) return country;

                country = new Country { CountryName = name, CountryCode = code };
                db.Countries.Add(country);
                db.SaveChanges();
                return country;
            }

            void EnsureCity(string cityName, Country country)
            {
                if (db.Cities.Any(c => c.CityName == cityName && c.CountryId == country.CountryId)) return;
                db.Cities.Add(new City { CityName = cityName, CountryId = country.CountryId });
                db.SaveChanges();
            }

            var egypt = EnsureCountry("Egypt", "EG");
            var usa = EnsureCountry("United States", "US");
            var uk = EnsureCountry("United Kingdom", "GB");
            var uae = EnsureCountry("United Arab Emirates", "AE");
            var france = EnsureCountry("France", "FR");
            var germany = EnsureCountry("Germany", "DE");
            var saudi = EnsureCountry("Saudi Arabia", "SA");
            var qatar = EnsureCountry("Qatar", "QA");
            var singapore = EnsureCountry("Singapore", "SG");
            var japan = EnsureCountry("Japan", "JP");

            EnsureCity("Cairo", egypt);
            EnsureCity("Alexandria", egypt);
            EnsureCity("New York", usa);
            EnsureCity("San Francisco", usa);
            EnsureCity("London", uk);
            EnsureCity("Manchester", uk);
            EnsureCity("Dubai", uae);
            EnsureCity("Abu Dhabi", uae);
            EnsureCity("Paris", france);
            EnsureCity("Berlin", germany);
            EnsureCity("Riyadh", saudi);
            EnsureCity("Jeddah", saudi);
            EnsureCity("Doha", qatar);
            EnsureCity("Singapore", singapore);
            EnsureCity("Tokyo", japan);

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
            EnsureUser("HR Supervisor", "hr@company.com", "E1004", "HR");
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

            void EnsureCurrency(string code, string name, decimal rate)
            {
                if (db.Currencies.Any(c => c.CurrencyCode == code)) return;
                db.Currencies.Add(new Currency
                {
                    CurrencyCode = code,
                    CurrencyName = name,
                    ExchangeRate = rate,
                    LastUpdated = DateTime.UtcNow
                });
                db.SaveChanges();
            }

            EnsureCurrency("EGP", "Egyptian Pound", 48m);
            EnsureCurrency("AED", "UAE Dirham", 3.67m);

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

            void EnsureExpenseCategory(string name, string description)
            {
                if (db.ExpenseCategories.Any(c => c.CategoryName == name)) return;
                db.ExpenseCategories.Add(new ExpenseCategory
                {
                    CategoryName = name,
                    Description = description,
                    IsActive = true
                });
                db.SaveChanges();
            }

            EnsureExpenseCategory("Flights", "Airfare and flight fees");
            EnsureExpenseCategory("Hotels", "Hotel bookings and lodging");
            EnsureExpenseCategory("Per Diem", "Daily allowance approved by HR");

            if (!db.Airlines.Any())
            {
                var airlines = new[]
                {
                    new Airline { AirlineName = "ExampleAir", AirlineCode = "EA", IsActive = true },
                    new Airline { AirlineName = "GlobalAir", AirlineCode = "GA", IsActive = true },
                    new Airline { AirlineName = "EgyptAir", AirlineCode = "MS", IsActive = true },
                    new Airline { AirlineName = "Emirates", AirlineCode = "EK", IsActive = true },
                    new Airline { AirlineName = "Qatar Airways", AirlineCode = "QR", IsActive = true }
                };
                db.Airlines.AddRange(airlines);
                db.SaveChanges();
            }

            void EnsureAirline(string name, string code)
            {
                if (db.Airlines.Any(a => a.AirlineCode == code)) return;
                db.Airlines.Add(new Airline
                {
                    AirlineName = name,
                    AirlineCode = code,
                    IsActive = true
                });
                db.SaveChanges();
            }

            EnsureAirline("EgyptAir", "MS");
            EnsureAirline("Emirates", "EK");
            EnsureAirline("Qatar Airways", "QR");

            if (!db.Hotels.Any())
            {
                City CityByName(string name) => db.Cities.First(c => c.CityName == name);

                db.Hotels.AddRange(
                    new Hotel { HotelName = "Cairo Business Hotel", CityId = CityByName("Cairo").CityId, Address = "Central Cairo", Rating = 4, Phone = "+20-2-0000", IsActive = true },
                    new Hotel { HotelName = "Dubai Executive Suites", CityId = CityByName("Dubai").CityId, Address = "Business Bay", Rating = 5, Phone = "+971-4-0000", IsActive = true },
                    new Hotel { HotelName = "London Corporate Inn", CityId = CityByName("London").CityId, Address = "Canary Wharf", Rating = 4, Phone = "+44-20-0000", IsActive = true },
                    new Hotel { HotelName = "New York Midtown Hotel", CityId = CityByName("New York").CityId, Address = "Midtown", Rating = 4, Phone = "+1-212-0000", IsActive = true },
                    new Hotel { HotelName = "Singapore Workstay", CityId = CityByName("Singapore").CityId, Address = "Marina district", Rating = 4, Phone = "+65-0000", IsActive = true }
                );
                db.SaveChanges();
            }

            void EnsureHotel(string name, string cityName, string address, int rating, string phone)
            {
                if (db.Hotels.Any(h => h.HotelName == name)) return;
                var city = db.Cities.First(c => c.CityName == cityName);
                db.Hotels.Add(new Hotel
                {
                    HotelName = name,
                    CityId = city.CityId,
                    Address = address,
                    Rating = rating,
                    Phone = phone,
                    IsActive = true
                });
                db.SaveChanges();
            }

            EnsureHotel("Cairo Business Hotel", "Cairo", "Central Cairo", 4, "+20-2-0000");
            EnsureHotel("Dubai Executive Suites", "Dubai", "Business Bay", 5, "+971-4-0000");
            EnsureHotel("London Corporate Inn", "London", "Canary Wharf", 4, "+44-20-0000");
            EnsureHotel("New York Midtown Hotel", "New York", "Midtown", 4, "+1-212-0000");
            EnsureHotel("Singapore Workstay", "Singapore", "Marina district", 4, "+65-0000");

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
