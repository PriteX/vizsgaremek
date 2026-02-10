using Idopontfoglalo.Core.Exceptions;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Idopontfoglalo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Idopontfoglalo.Infrastructure.Services;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly AppDbContext _db;

    public ServiceCatalogService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ServiceDto>> GetAllAsync()
    {
        return await _db.Services
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto(s.Id, s.Name, s.Description, s.DurationMinutes, s.Price, s.IsActive))
            .ToListAsync();
    }

    public async Task<ServiceDto?> GetByIdAsync(int id)
    {
        return await _db.Services
            .Where(s => s.Id == id)
            .Select(s => new ServiceDto(s.Id, s.Name, s.Description, s.DurationMinutes, s.Price, s.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceDto> CreateAsync(ServiceUpsertModel model)
    {
        if (model.DurationMinutes <= 0)
            throw new BusinessException("A szolgáltatás időtartama legyen pozitív szám.");

        var entity = new Idopontfoglalo.Core.Entities.Service
        {
            Name = model.Name.Trim(),
            Description = model.Description,
            DurationMinutes = model.DurationMinutes,
            Price = model.Price,
            IsActive = model.IsActive
        };

        _db.Services.Add(entity);
        await _db.SaveChangesAsync();

        return new ServiceDto(entity.Id, entity.Name, entity.Description, entity.DurationMinutes, entity.Price, entity.IsActive);
    }

    public async Task<ServiceDto> UpdateAsync(int id, ServiceUpsertModel model)
    {
        var entity = await _db.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (entity is null)
            throw new BusinessException("A szolgáltatás nem található.");

        entity.Name = model.Name.Trim();
        entity.Description = model.Description;
        entity.DurationMinutes = model.DurationMinutes;
        entity.Price = model.Price;
        entity.IsActive = model.IsActive;

        await _db.SaveChangesAsync();
        return new ServiceDto(entity.Id, entity.Name, entity.Description, entity.DurationMinutes, entity.Price, entity.IsActive);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (entity is null)
            return;

        // Vizsgaremekben általában elég a soft delete:
        entity.IsActive = false;
        await _db.SaveChangesAsync();
    }
}
