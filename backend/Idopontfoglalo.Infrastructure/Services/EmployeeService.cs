using Idopontfoglalo.Core.Entities;
using Idopontfoglalo.Core.Exceptions;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Idopontfoglalo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Idopontfoglalo.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _db;

    public EmployeeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<EmployeeDto>> GetAllAsync()
    {
        return await _db.Employees
            .OrderBy(e => e.Name)
                      .Select(e => new EmployeeDto(e.Id, e.Name, e.Email, e.Phone, e.IsActive, e.LocationId, e.Location != null ? e.Location.Name : null))
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        return await _db.Employees
            .Where(e => e.Id == id)
                       .Select(e => new EmployeeDto(e.Id, e.Name, e.Email, e.Phone, e.IsActive, e.LocationId, e.Location != null ? e.Location.Name : null))
            .FirstOrDefaultAsync();
    }
    public async Task<List<int>> GetServiceIdsAsync(int employeeId)
    {
        var employeeExists = await _db.Employees.AnyAsync(e => e.Id == employeeId);
        if (!employeeExists)
            throw new BusinessException("A dolgozó nem található.");

        return await _db.EmployeeServices
            .Where(es => es.EmployeeId == employeeId)
            .Select(es => es.ServiceId)
            .OrderBy(id => id)
            .ToListAsync();
    }

    public async Task SetServicesAsync(int employeeId, List<int> serviceIds)
    {
        var employeeExists = await _db.Employees.AnyAsync(e => e.Id == employeeId);
        if (!employeeExists)
            throw new BusinessException("A dolgozó nem található.");

        var normalizedIds = (serviceIds ?? new List<int>())
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (normalizedIds.Count > 0)
        {
            var existingServiceCount = await _db.Services.CountAsync(s => normalizedIds.Contains(s.Id) && s.IsActive);
            if (existingServiceCount != normalizedIds.Count)
                throw new BusinessException("A megadott szolgáltatások között érvénytelen vagy inaktív elem található.");
        }

        var currentLinks = await _db.EmployeeServices
            .Where(es => es.EmployeeId == employeeId)
            .ToListAsync();

        _db.EmployeeServices.RemoveRange(currentLinks);

        foreach (var serviceId in normalizedIds)
        {
            _db.EmployeeServices.Add(new Core.Entities.EmployeeServiceService
            {
                EmployeeId = employeeId,
                ServiceId = serviceId
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task<EmployeeDto> CreateAsync(EmployeeUpsertModel model)
    {
        await ValidateLocationAsync(model.LocationId);
        var entity = new Idopontfoglalo.Core.Entities.Employee
        {
            Name = model.Name.Trim(),
            Email = model.Email,
            Phone = model.Phone,
            IsActive = model.IsActive,
            LocationId = model.LocationId
        };

        _db.Employees.Add(entity);
        await _db.SaveChangesAsync();

        var defaultAvailability = Enumerable.Range(1, 5)
            .Select(dayOfWeek => new Availability
            {
                EmployeeId = entity.Id,
                DayOfWeek = dayOfWeek,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17),
                IsActive = true
            });

        _db.Availability.AddRange(defaultAvailability);
        await _db.SaveChangesAsync();


        return new EmployeeDto(entity.Id, entity.Name, entity.Email, entity.Phone, entity.IsActive, entity.LocationId, null);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, EmployeeUpsertModel model)
    {
        await ValidateLocationAsync(model.LocationId);
        var entity = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null)
            throw new BusinessException("A dolgozó nem található.");

        entity.Name = model.Name.Trim();
        entity.Email = model.Email;
        entity.Phone = model.Phone;
        entity.IsActive = model.IsActive;
        entity.LocationId = model.LocationId;

        await _db.SaveChangesAsync();
        return new EmployeeDto(entity.Id, entity.Name, entity.Email, entity.Phone, entity.IsActive, entity.LocationId, null);
    }


    private async Task ValidateLocationAsync(int? locationId)
    {
        if (!locationId.HasValue)
            return;

        var exists = await _db.Locations.AnyAsync(l => l.Id == locationId.Value && l.IsActive);
        if (!exists)
            throw new BusinessException("A megadott helyszín nem létezik vagy inaktív.");
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null)
            return;

        var hasAppointments = await _db.Appointments.AnyAsync(a => a.EmployeeId == id);
        if (hasAppointments)
            throw new BusinessException("A dolgozóhoz tartozik foglalás, ezért nem törölhető.");

        var availabilities = await _db.Availability
            .Where(a => a.EmployeeId == id)
            .ToListAsync();
        _db.Availability.RemoveRange(availabilities);

        var serviceLinks = await _db.EmployeeServices
            .Where(es => es.EmployeeId == id)
            .ToListAsync();
        _db.EmployeeServices.RemoveRange(serviceLinks);

        _db.Employees.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
