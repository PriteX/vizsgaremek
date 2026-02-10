using Idopontfoglalo.Core.Entities;
using Idopontfoglalo.Infrastructure.Services;
using Xunit;

namespace Idopontfoglalo.Tests;

public class SlotServiceTests
{
    [Fact]
    public async Task GetSlots_ExcludesBookedAppointment()
    {
        var db = TestDbFactory.CreateContext();

       
        var date = new DateOnly(2030, 2, 4);

        
        db.Appointments.Add(new Appointment
        {
            Id = 1,
            UserId = 1,
            EmployeeId = 1,
            ServiceId = 1,
            StartAt = date.ToDateTime(new TimeOnly(10, 0)),
            EndAt = date.ToDateTime(new TimeOnly(10, 30)),
            Status = AppointmentStatus.Booked
        });
        db.SaveChanges();

        var availability = new AvailabilityService(db);

        var slots = await availability.GetSlotsAsync(employeeId: 1, serviceId: 1, date: date);

        Assert.DoesNotContain(slots, s => s.StartAt == date.ToDateTime(new TimeOnly(10, 0)));

        
        Assert.Contains(slots, s => s.StartAt == date.ToDateTime(new TimeOnly(9, 0)));
    }
}
