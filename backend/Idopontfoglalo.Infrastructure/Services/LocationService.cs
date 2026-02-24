using Idopontfoglalo.Core.Entities;
using Idopontfoglalo.Core.Exceptions;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Idopontfoglalo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Idopontfoglalo.Infrastructure.Services;

public class LocationService : ILocationService
{
    private readonly AppDbContext _db;

    public LocationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LocationDto>> GetAllAsync()
        => await _db.Locations
            .OrderBy(l => l.Name)
            .Select(l => new LocationDto(l.Id, l.Name, l.IsActive))
            .ToListAsync();

    public async Task<LocationDto?> GetByIdAsync(int id)
        => await _db.Locations
            .Where(l => l.Id == id)
            .Select(l => new LocationDto(l.Id, l.Name, l.IsActive))
            .FirstOrDefaultAsync();

    public async Task<LocationDto> CreateAsync(LocationUpsertModel model)
    {
        var name = model.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("A helyszín neve kötelező.");

        var exists = await _db.Locations.AnyAsync(l => l.Name == name);
        if (exists)
            throw new BusinessException("Ilyen nevű helyszín már létezik.");

        var entity = new Location { Name = name, IsActive = model.IsActive };
        _db.Locations.Add(entity);
        await _db.SaveChangesAsync();

        return new LocationDto(entity.Id, entity.Name, entity.IsActive);
    }

    public async Task<LocationDto> UpdateAsync(int id, LocationUpsertModel model)
    {
        var entity = await _db.Locations.FirstOrDefaultAsync(l => l.Id == id);
        if (entity is null)
            throw new BusinessException("A helyszín nem található.");

        var name = model.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("A helyszín neve kötelező.");

        var exists = await _db.Locations.AnyAsync(l => l.Id != id && l.Name == name);
        if (exists)
            throw new BusinessException("Ilyen nevű helyszín már létezik.");

        entity.Name = name;
        entity.IsActive = model.IsActive;
        await _db.SaveChangesAsync();

        return new LocationDto(entity.Id, entity.Name, entity.IsActive);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Locations.FirstOrDefaultAsync(l => l.Id == id);
        if (entity is null)
            return;

        entity.IsActive = false;
        await _db.SaveChangesAsync();
    }
}