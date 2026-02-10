namespace Idopontfoglalo.Api.Contracts;

public record ServiceUpsertRequest(string Name, string? Description, int DurationMinutes, decimal Price, bool IsActive);
