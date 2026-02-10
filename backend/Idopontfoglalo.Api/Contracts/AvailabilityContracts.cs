namespace Idopontfoglalo.Api.Contracts;

public record AvailabilityUpsertRequest(
    int EmployeeId,
    int DayOfWeek,
    string StartTime, // "09:00"
    string EndTime,   // "17:00"
    string? ValidFrom,
    string? ValidTo,
    bool IsActive
);
