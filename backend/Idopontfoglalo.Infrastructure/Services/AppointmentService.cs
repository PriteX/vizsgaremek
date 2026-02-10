using Idopontfoglalo.Core.Entities;
using Idopontfoglalo.Core.Exceptions;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Idopontfoglalo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Idopontfoglalo.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _db;
    private readonly IAvailabilityService _availability;

    public AppointmentService(AppDbContext db, IAvailabilityService availability)
    {
        _db = db;
        _availability = availability;
    }

    public async Task<AppointmentDto> CreateAsync(int userId, AppointmentCreateModel model)
    {
        var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == model.ServiceId && s.IsActive);
        if (service is null)
            throw new BusinessException("A szolgáltatás nem található.");

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == model.EmployeeId && e.IsActive);
        if (employee is null)
            throw new BusinessException("A dolgozó nem található.");

        var provides = await _db.EmployeeServices.AnyAsync(es => es.EmployeeId == model.EmployeeId && es.ServiceId == model.ServiceId);
        if (!provides)
            throw new BusinessException("A kiválasztott dolgozó nem nyújtja ezt a szolgáltatást.");

        var startAt = DateTime.SpecifyKind(model.StartAt, DateTimeKind.Utc);
        if (startAt < DateTime.UtcNow.AddMinutes(-1))
            throw new BusinessException("Múltbeli időpont nem foglalható.");

        var endAt = startAt.AddMinutes(service.DurationMinutes);

        
        var date = DateOnly.FromDateTime(startAt);
        var slots = await _availability.GetSlotsAsync(model.EmployeeId, model.ServiceId, date);
        var slotOk = slots.Any(s => s.StartAt == startAt && s.EndAt == endAt);
        if (!slotOk)
            throw new BusinessException("A kiválasztott időpont már nem elérhető.");

       
        var entity = new Appointment
        {
            UserId = userId,
            EmployeeId = model.EmployeeId,
            ServiceId = model.ServiceId,
            StartAt = startAt,
            EndAt = endAt,
            Status = AppointmentStatus.Booked
        };

        _db.Appointments.Add(entity);
        await _db.SaveChangesAsync();

        return new AppointmentDto(entity.Id, entity.UserId, entity.EmployeeId, entity.ServiceId, entity.StartAt, entity.EndAt, entity.Status);
    }

    public async Task<List<AppointmentDto>> GetMyAppointmentsAsync(int userId)
    {
        return await _db.Appointments
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.StartAt)
            .Select(a => new AppointmentDto(a.Id, a.UserId, a.EmployeeId, a.ServiceId, a.StartAt, a.EndAt, a.Status))
            .ToListAsync();
    }

    public async Task CancelAsync(int userId, int appointmentId)
    {
        var entity = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
        if (entity is null)
            return;

        if (entity.UserId != userId)
            throw new BusinessException("Nincs jogosultság a foglalás lemondásához.");

        entity.Status = AppointmentStatus.Cancelled;
        await _db.SaveChangesAsync();
    }

    public async Task<List<AppointmentDto>> GetAppointmentsForRangeAsync(DateOnly from, DateOnly to)
    {
        var fromDt = from.ToDateTime(new TimeOnly(0, 0));
        var toDt = to.ToDateTime(new TimeOnly(23, 59));

        return await _db.Appointments
            .Where(a => a.StartAt >= fromDt && a.StartAt <= toDt)
            .OrderBy(a => a.StartAt)
            .Select(a => new AppointmentDto(a.Id, a.UserId, a.EmployeeId, a.ServiceId, a.StartAt, a.EndAt, a.Status))
            .ToListAsync();
    }
}
