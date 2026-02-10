using Idopontfoglalo.Api.Contracts;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Idopontfoglalo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _services;

    public ServicesController(IServiceCatalogService services)
    {
        _services = services;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ServiceDto>>> GetAll()
        => Ok(await _services.GetAllAsync());

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceDto>> GetById([FromRoute] int id)
    {
        var item = await _services.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ServiceDto>> Create([FromBody] ServiceUpsertRequest req)
    {
        var created = await _services.CreateAsync(new ServiceUpsertModel(req.Name, req.Description, req.DurationMinutes, req.Price, req.IsActive));
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ServiceDto>> Update([FromRoute] int id, [FromBody] ServiceUpsertRequest req)
    {
        var updated = await _services.UpdateAsync(id, new ServiceUpsertModel(req.Name, req.Description, req.DurationMinutes, req.Price, req.IsActive));
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _services.DeleteAsync(id);
        return NoContent();
    }
}
