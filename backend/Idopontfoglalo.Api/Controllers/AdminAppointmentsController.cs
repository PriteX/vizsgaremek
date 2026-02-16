using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Idopontfoglalo.Api.Controllers;

[ApiController]
[Route("api/admin/appointments")]
[Authorize(Roles = "ADMIN")]
public class AdminAppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;
    private readonly AppDbContext _db;

    public AdminAppointmentsController(IAppointmentService appointments, AppDbContext db)
    {
        _appointments = appointments;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminAppointmentDto>>> GetForRange([FromQuery] DateOnly dateFrom, [FromQuery] DateOnly dateTo)
    {
        var appointments = await _appointments.GetAppointmentsForRangeAsync(dateFrom, dateTo);

        var users = await _db.Users
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
            .ToListAsync();

        var userNames = users.ToDictionary(
            u => u.Id,
            u =>
            {
                var fullName = string.Join(" ", new[] { u.LastName, u.FirstName }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

                return string.IsNullOrWhiteSpace(fullName) ? u.Email : fullName;
            }
        );

        var employeeNames = await _db.Employees
            .Select(e => new { e.Id, e.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name);

        var serviceNames = await _db.Services
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name);

        var response = appointments.Select(a => new AdminAppointmentDto(
            a.Id,
            userNames.TryGetValue(a.UserId, out var userName) ? userName : $"Felhasználó #{a.UserId}",
            employeeNames.TryGetValue(a.EmployeeId, out var employeeName) ? employeeName : $"Dolgozó #{a.EmployeeId}",
            serviceNames.TryGetValue(a.ServiceId, out var serviceName) ? serviceName : $"Szolgáltatás #{a.ServiceId}",
            a.StartAt,
            a.EndAt,
            a.Status.ToString()
        )).ToList();

        return Ok(response);
    }

    public record AdminAppointmentDto(
        int Id,
        string UserName,
        string EmployeeName,
        string ServiceName,
        DateTime StartAt,
        DateTime EndAt,
        string Status
    );
}
