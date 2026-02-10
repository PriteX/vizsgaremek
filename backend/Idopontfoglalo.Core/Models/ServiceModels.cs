namespace Idopontfoglalo.Core.Models;

public record ServiceDto(int Id, string Name, string? Description, int DurationMinutes, decimal Price, bool IsActive);
public record ServiceUpsertModel(string Name, string? Description, int DurationMinutes, decimal Price, bool IsActive);
