using Instrux.Services.DTOs;

namespace Instrux.Services.Interfaces;

public interface IAuthenticationService
{
    Task<AuthResultDto> LoginAsync(LoginRequestDto request);
    Task<AuthResultDto> RegisterAsync(RegisterRequestDto request);
}
