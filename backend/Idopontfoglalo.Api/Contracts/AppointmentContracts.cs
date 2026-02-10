namespace Idopontfoglalo.Api.Contracts;

public record AppointmentCreateRequest(int EmployeeId, int ServiceId, DateTime StartAt);
