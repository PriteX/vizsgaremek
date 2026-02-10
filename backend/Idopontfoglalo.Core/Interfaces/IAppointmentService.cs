using Idopontfoglalo.Core.Models;

namespace Idopontfoglalo.Core.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentDto> CreateAsync(int userId, AppointmentCreateModel model);
    Task<List<AppointmentDto>> GetMyAppointmentsAsync(int userId);
    Task CancelAsync(int userId, int appointmentId);

    Task<List<AppointmentDto>> GetAppointmentsForRangeAsync(DateOnly from, DateOnly to);
}
