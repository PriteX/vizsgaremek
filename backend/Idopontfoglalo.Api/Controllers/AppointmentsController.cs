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
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = int.Parse(userIdStr);
        var created = await _appointments.CreateAsync(userId, new AppointmentCreateModel(req.EmployeeId, req.ServiceId, req.StartAt));
        return Ok(created);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<List<AppointmentDto>>> MyAppointments()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = int.Parse(userIdStr);
        return Ok(await _appointments.GetMyAppointmentsAsync(userId));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Cancel([FromRoute] int id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = int.Parse(userIdStr);
        await _appointments.CancelAsync(userId, id);
        return NoContent();
    }
}
