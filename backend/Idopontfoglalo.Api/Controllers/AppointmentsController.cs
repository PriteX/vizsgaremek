using System.Security.Claims;
using Idopontfoglalo.Api.Contracts;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Idopontfoglalo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;

    public AppointmentsController(IAppointmentService appointments)
    {
        _appointments = appointments;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] AppointmentCreateRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var created = await _appointments.CreateAsync(userId, new AppointmentCreateModel(req.EmployeeId, req.ServiceId, req.StartAt));
        return Ok(created);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<List<AppointmentDto>>> MyAppointments()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _appointments.GetMyAppointmentsAsync(userId));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Cancel([FromRoute] int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _appointments.CancelAsync(userId, id);
        return NoContent();
    }
}
