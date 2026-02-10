using Idopontfoglalo.Core.Entities;

namespace Idopontfoglalo.Core.Models;

public record AppointmentDto(
    int Id,
    int UserId,
    int EmployeeId,
    int ServiceId,
    DateTime StartAt,
    DateTime EndAt,
    AppointmentStatus Status
);

public record AppointmentCreateModel(
    int EmployeeId,
    int ServiceId,
    DateTime StartAt
);
