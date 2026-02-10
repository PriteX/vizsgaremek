using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Idopontfoglalo.Core.Entities;
using Idopontfoglalo.Core.Exceptions;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Core.Models;
using Idopontfoglalo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Idopontfoglalo.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResult> RegisterAsync(RegisterModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new BusinessException("Ezzel az e-mail címmel már létezik felhasználó.");

        var userRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "USER");
        if (userRole is null)
            throw new BusinessException("Hiányzó USER szerepkör (seed adat).");

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            FirstName = model.FirstName,
            LastName = model.LastName,
            RoleId = userRole.Id
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // reload role
        user.Role = userRole;

        return new AuthResult(
            Token: CreateJwtToken(user),
            Email: user.Email,
            Role: userRole.Name
        );
    }

    public async Task<AuthResult> LoginAsync(LoginModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || user.Role is null)
            throw new BusinessException("Hibás e-mail vagy jelszó.");

        if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            throw new BusinessException("Hibás e-mail vagy jelszó.");

        return new AuthResult(
            Token: CreateJwtToken(user),
            Email: user.Email,
            Role: user.Role.Name
        );
    }

    private string CreateJwtToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new BusinessException("Hiányzó Jwt:Key beállítás.");
        var issuer = _config["Jwt:Issuer"] ?? "idopontfoglalo";
        var audience = _config["Jwt:Audience"] ?? "idopontfoglalo";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "USER"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
