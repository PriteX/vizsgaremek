using Idopontfoglalo.Core.Entities;
using Idopontfoglalo.Core.Exceptions;
using Idopontfoglalo.Infrastructure.Services;
using Xunit;

namespace Idopontfoglalo.Tests;

public class AppointmentServiceTests
{
    [Fact]
    public async Task CreateAppointment_FailsWhenSlotNotAvailable()
    {
        var db = TestDbFactory.CreateContext();
        var availability = new AvailabilityService(db);
        var service = new AppointmentService(db, availability);

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

        
        await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await service.CreateAsync(1, new Idopontfoglalo.Core.Models.AppointmentCreateModel(
                EmployeeId: 1,
                ServiceId: 1,
                StartAt: date.ToDateTime(new TimeOnly(10, 0))
            ));
        });
    }

    [Fact]
    public async Task CreateAppointment_SucceedsWhenSlotAvailable()
    {
        var db = TestDbFactory.CreateContext();
        var availability = new AvailabilityService(db);
        var service = new AppointmentService(db, availability);

        var date = new DateOnly(2030, 2, 4);

        var created = await service.CreateAsync(1, new Idopontfoglalo.Core.Models.AppointmentCreateModel(
            EmployeeId: 1,
            ServiceId: 1,
            StartAt: date.ToDateTime(new TimeOnly(9, 0))
        ));

        Assert.True(created.Id > 0);
        Assert.Equal(AppointmentStatus.Booked, created.Status);
        Assert.Equal(date.ToDateTime(new TimeOnly(9, 0)), created.StartAt);
    }
}
