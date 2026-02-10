namespace Idopontfoglalo.Core.Models;

public record EmployeeDto(int Id, string Name, string? Email, string? Phone, bool IsActive);
public record EmployeeUpsertModel(string Name, string? Email, string? Phone, bool IsActive);
