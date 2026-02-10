using Idopontfoglalo.Core.Models;

namespace Idopontfoglalo.Core.Interfaces;

public interface IServiceCatalogService
{
    Task<List<ServiceDto>> GetAllAsync();
    Task<ServiceDto?> GetByIdAsync(int id);
    Task<ServiceDto> CreateAsync(ServiceUpsertModel model);
    Task<ServiceDto> UpdateAsync(int id, ServiceUpsertModel model);
    Task DeleteAsync(int id);
}
