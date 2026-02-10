using Idopontfoglalo.Api.Contracts;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Idopontfoglalo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availability;

    public AvailabilityController(IAvailabilityService availability)
    {
        _availability = availability;
    }


    [HttpGet("slots")]
    [AllowAnonymous]
    public async Task<ActionResult<List<SlotDto>>> GetSlots([FromQuery] int employeeId, [FromQuery] int serviceId, [FromQuery] DateOnly date)
    {
        var slots = await _availability.GetSlotsAsync(employeeId, serviceId, date);
        return Ok(slots);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> AddWeeklyAvailability([FromBody] AvailabilityUpsertRequest req)
    {
        TimeSpan start = TimeSpan.Parse(req.StartTime);
        TimeSpan end = TimeSpan.Parse(req.EndTime);

        DateOnly? validFrom = string.IsNullOrWhiteSpace(req.ValidFrom) ? null : DateOnly.Parse(req.ValidFrom);
        DateOnly? validTo = string.IsNullOrWhiteSpace(req.ValidTo) ? null : DateOnly.Parse(req.ValidTo);

        await _availability.AddWeeklyAvailabilityAsync(new AvailabilityUpsertModel(
            req.EmployeeId,
            req.DayOfWeek,
            start,
            end,
            validFrom,
            validTo,
            req.IsActive
        ));

        return NoContent();
    }
}
