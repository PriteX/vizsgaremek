using Idopontfoglalo.Core.Models;

namespace Idopontfoglalo.Core.Interfaces;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto> CreateAsync(EmployeeUpsertModel model);
    Task<EmployeeDto> UpdateAsync(int id, EmployeeUpsertModel model);
    Task DeleteAsync(int id);
    Task<List<int>> GetServiceIdsAsync(int employeeId);
    Task SetServicesAsync(int employeeId, List<int> serviceIds);
}
