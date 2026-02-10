using Idopontfoglalo.Core.Models;

namespace Idopontfoglalo.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterModel model);
    Task<AuthResult> LoginAsync(LoginModel model);
}
