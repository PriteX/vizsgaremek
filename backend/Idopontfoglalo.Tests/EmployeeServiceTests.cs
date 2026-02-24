using Idopontfoglalo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Idopontfoglalo.Tests;

public class EmployeeServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesDefaultWeekdayAvailability()
    {
        var db = TestDbFactory.CreateContext();
        var service = new EmployeeService(db);

        var created = await service.CreateAsync(new Idopontfoglalo.Core.Models.EmployeeUpsertModel(
            Name: "Teszt Dolgozó",
            Email: "teszt@demo.local",
            Phone: "+36 30 123 4567",
            IsActive: true,
            LocationId: 1
        ));

        var availabilities = await db.Availability
            .Where(a => a.EmployeeId == created.Id && a.IsActive)
            .OrderBy(a => a.DayOfWeek)
            .ToListAsync();

        Assert.Equal(5, availabilities.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, availabilities.Select(a => a.DayOfWeek).ToArray());
        Assert.All(availabilities, a =>
        {
            Assert.Equal(TimeSpan.FromHours(9), a.StartTime);
            Assert.Equal(TimeSpan.FromHours(17), a.EndTime);
        });

        var serviceLinks = await db.EmployeeServices
            .Where(es => es.EmployeeId == created.Id)
            .ToListAsync();

        Assert.Equal(2, serviceLinks.Count);
        Assert.Contains(serviceLinks, es => es.ServiceId == 1);
        Assert.Contains(serviceLinks, es => es.ServiceId == 2);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEmployeeWithoutAppointments()
    {
        var db = TestDbFactory.CreateContext();
        var service = new EmployeeService(db);

        await service.DeleteAsync(1);

        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == 1);
        Assert.Null(employee);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenEmployeeHasAppointments()
    {
        var db = TestDbFactory.CreateContext();
        db.Appointments.Add(new Idopontfoglalo.Core.Entities.Appointment
        {
            Id = 99,
            UserId = 1,
            EmployeeId = 1,
            ServiceId = 1,
            StartAt = new DateTime(2030, 2, 4, 9, 0, 0),
            EndAt = new DateTime(2030, 2, 4, 9, 30, 0),
            Status = Idopontfoglalo.Core.Entities.AppointmentStatus.Booked
        });
        await db.SaveChangesAsync();

        var service = new EmployeeService(db);

        await Assert.ThrowsAsync<Idopontfoglalo.Core.Exceptions.BusinessException>(() => service.DeleteAsync(1));
    }
}