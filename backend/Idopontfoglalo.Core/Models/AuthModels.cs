namespace Idopontfoglalo.Core.Models;

public record RegisterModel(string Email, string Password, string? FirstName, string? LastName);
public record LoginModel(string Email, string Password);

public record AuthResult(string Token, string Email, string Role);
