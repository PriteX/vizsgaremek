using Idopontfoglalo.Core.Entities;
using Idopontfoglalo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Idopontfoglalo.Tests;

public static class TestDbFactory
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);

        // Seed minimal data
        var adminRole = new Role { Id = 1, Name = "ADMIN" };
        var userRole = new Role { Id = 2, Name = "USER" };
        db.Roles.AddRange(adminRole, userRole);

        db.Users.Add(new User { Id = 1, Email = "user@demo.local", PasswordHash = "x", RoleId = 2, Role = userRole });
        db.Locations.AddRange(
    new Location { Id = 1, Name = "Fodrászat", IsActive = true },
    new Location { Id = 2, Name = "Kozmetika", IsActive = true }
);


        db.Services.AddRange(
            new Service { Id = 1, Name = "Hajvágás", DurationMinutes = 30, Price = 4500, IsActive = true },
            new Service { Id = 2, Name = "Szakáll", DurationMinutes = 15, Price = 2500, IsActive = true }
        );

        db.Employees.Add(new Employee { Id = 1, Name = "Kiss Anna", IsActive = true, LocationId = 1 });

        db.EmployeeServices.AddRange(
            new EmployeeServiceService { EmployeeId = 1, ServiceId = 1 },
            new EmployeeServiceService { EmployeeId = 1, ServiceId = 2 }
        );

        
        db.Availability.Add(new Availability
        {
            Id = 1,
            EmployeeId = 1,
            DayOfWeek = 1,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(12),
            IsActive = true
        });

        db.SaveChanges();
        return db;
    }
}
