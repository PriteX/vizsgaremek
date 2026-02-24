namespace Idopontfoglalo.Core.Models;

public record LocationDto(int Id, string Name, bool IsActive);
public record LocationUpsertModel(string Name, bool IsActive);