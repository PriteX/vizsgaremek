namespace Idopontfoglalo.Core.Models;

public record SlotDto(DateTime StartAt, DateTime EndAt);

public record AvailabilityUpsertModel(
    int EmployeeId,
    int DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    bool IsActive
);
