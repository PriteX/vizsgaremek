using Idopontfoglalo.Core.Models;

namespace Idopontfoglalo.Core.Interfaces;

public interface ILocationService
{
    Task<List<LocationDto>> GetAllAsync();
    Task<LocationDto?> GetByIdAsync(int id);
    Task<LocationDto> CreateAsync(LocationUpsertModel model);
    Task<LocationDto> UpdateAsync(int id, LocationUpsertModel model);
    Task DeleteAsync(int id);
}