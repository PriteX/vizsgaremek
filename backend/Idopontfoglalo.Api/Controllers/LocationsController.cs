using Idopontfoglalo.Api.Contracts;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Idopontfoglalo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locations;

    public LocationsController(ILocationService locations)
    {
        _locations = locations;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<LocationDto>>> GetAll()
        => Ok(await _locations.GetAllAsync());

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<LocationDto>> GetById([FromRoute] int id)
    {
        var item = await _locations.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<LocationDto>> Create([FromBody] LocationUpsertRequest req)
    {
        var created = await _locations.CreateAsync(new LocationUpsertModel(req.Name, req.IsActive));
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<LocationDto>> Update([FromRoute] int id, [FromBody] LocationUpsertRequest req)
    {
        var updated = await _locations.UpdateAsync(id, new LocationUpsertModel(req.Name, req.IsActive));
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _locations.DeleteAsync(id);
        return NoContent();
    }
}