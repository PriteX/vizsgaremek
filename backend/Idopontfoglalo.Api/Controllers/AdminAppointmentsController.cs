using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Idopontfoglalo.Api.Controllers;

[ApiController]
[Route("api/admin/appointments")]
[Authorize(Roles = "ADMIN")]
public class AdminAppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;

    public AdminAppointmentsController(IAppointmentService appointments)
    {
        _appointments = appointments;
    }

    [HttpGet]
    public async Task<ActionResult<List<AppointmentDto>>> GetForRange([FromQuery] DateOnly dateFrom, [FromQuery] DateOnly dateTo)
    {
        return Ok(await _appointments.GetAppointmentsForRangeAsync(dateFrom, dateTo));
    }
}
