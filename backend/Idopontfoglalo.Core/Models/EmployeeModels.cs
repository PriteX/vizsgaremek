namespace Idopontfoglalo.Core.Models;

public record EmployeeDto(int Id, string Name, string? Email, string? Phone, bool IsActive, int? LocationId, string? LocationName);
public record EmployeeUpsertModel(string Name, string? Email, string? Phone, bool IsActive, int? LocationId);