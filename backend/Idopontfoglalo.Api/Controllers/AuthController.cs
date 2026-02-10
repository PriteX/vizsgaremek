using Idopontfoglalo.Api.Contracts;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Idopontfoglalo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        var result = await _auth.RegisterAsync(new RegisterModel(req.Email, req.Password, req.FirstName, req.LastName));
        return Ok(new AuthResponse(result.Token, result.Email, result.Role));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var result = await _auth.LoginAsync(new LoginModel(req.Email, req.Password));
        return Ok(new AuthResponse(result.Token, result.Email, result.Role));
    }
}
