using Idopontfoglalo.Api.Contracts;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Idopontfoglalo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employees;

    public EmployeesController(IEmployeeService employees)
    {
        _employees = employees;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll()
        => Ok(await _employees.GetAllAsync());

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<EmployeeDto>> GetById([FromRoute] int id)
    {
        var item = await _employees.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }
    [HttpGet("{id:int}/services")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<List<int>>> GetServices([FromRoute] int id)
        => Ok(await _employees.GetServiceIdsAsync(id));

    [HttpPut("{id:int}/services")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> SetServices([FromRoute] int id, [FromBody] EmployeeServicesUpdateRequest req)
    {
        await _employees.SetServicesAsync(id, req.ServiceIds ?? new List<int>());
        return NoContent();
    }


    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] EmployeeUpsertRequest req)
    {
        var created = await _employees.CreateAsync(new EmployeeUpsertModel(req.Name, req.Email, req.Phone, req.IsActive));
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<EmployeeDto>> Update([FromRoute] int id, [FromBody] EmployeeUpsertRequest req)
    {
        var updated = await _employees.UpdateAsync(id, new EmployeeUpsertModel(req.Name, req.Email, req.Phone, req.IsActive));
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _employees.DeleteAsync(id);
        return NoContent();
    }
}
