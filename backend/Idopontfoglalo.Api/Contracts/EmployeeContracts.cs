namespace Idopontfoglalo.Api.Contracts;

public record EmployeeUpsertRequest(string Name, string? Email, string? Phone, bool IsActive);

public record EmployeeServicesUpdateRequest(List<int> ServiceIds);
