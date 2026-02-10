using Idopontfoglalo.Core.Entities;
using Idopontfoglalo.Core.Exceptions;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Idopontfoglalo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Idopontfoglalo.Infrastructure.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly AppDbContext _db;

    public AvailabilityService(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddWeeklyAvailabilityAsync(AvailabilityUpsertModel model)
    {
        if (model.EndTime <= model.StartTime)
            throw new BusinessException("A munkaidő vége legyen később, mint a kezdete.");

        var employeeExists = await _db.Employees.AnyAsync(e => e.Id == model.EmployeeId);
        if (!employeeExists)
            throw new BusinessException("Ismeretlen dolgozó.");

        var entity = new Availability
        {
            EmployeeId = model.EmployeeId,
            DayOfWeek = model.DayOfWeek,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            IsActive = model.IsActive
        };

        _db.Availability.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<List<SlotDto>> GetSlotsAsync(int employeeId, int serviceId, DateOnly date)
    {
        var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive);
        if (service is null)
            throw new BusinessException("A szolgáltatás nem található.");

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive);
        if (employee is null)
            throw new BusinessException("A dolgozó nem található.");

        var provides = await _db.EmployeeServices.AnyAsync(es => es.EmployeeId == employeeId && es.ServiceId == serviceId);
        if (!provides)
            throw new BusinessException("A kiválasztott dolgozó nem nyújtja ezt a szolgáltatást.");

        var dow = (int)date.DayOfWeek; // Sunday=0 ... Saturday=6

        var avs = await _db.Availability
            .Where(a => a.EmployeeId == employeeId
                        && a.IsActive
                        && a.DayOfWeek == dow
                        && (a.ValidFrom == null || a.ValidFrom <= date)
                        && (a.ValidTo == null || a.ValidTo >= date))
            .ToListAsync();

        if (avs.Count == 0)
            return new List<SlotDto>();

       
        var dayStart = date.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = date.ToDateTime(new TimeOnly(23, 59));

        var booked = await _db.Appointments
            .Where(a => a.EmployeeId == employeeId
                        && a.Status == AppointmentStatus.Booked
                        && a.StartAt < dayEnd
                        && a.EndAt > dayStart)
            .ToListAsync();

        var slots = new List<SlotDto>();
        var duration = TimeSpan.FromMinutes(service.DurationMinutes);
        var step = TimeSpan.FromMinutes(15);

        foreach (var av in avs)
        {
            var windowStart = date.ToDateTime(TimeOnly.FromTimeSpan(av.StartTime));
            var windowEnd = date.ToDateTime(TimeOnly.FromTimeSpan(av.EndTime));

            for (var t = windowStart; t + duration <= windowEnd; t = t.Add(step))
            {
                var slotStart = t;
                var slotEnd = t.Add(duration);

                var overlaps = booked.Any(a => slotStart < a.EndAt && slotEnd > a.StartAt);
                if (!overlaps)
                    slots.Add(new SlotDto(slotStart, slotEnd));
            }
        }

        return slots.OrderBy(s => s.StartAt).ToList();
    }
}
