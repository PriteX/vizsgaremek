namespace Idopontfoglalo.Api.Contracts;

public record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);
public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, string Email, string Role);
