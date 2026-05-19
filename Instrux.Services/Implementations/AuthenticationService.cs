using Instrux.Domain.Models;
using Instrux.Infrastructure.Repositories;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;

namespace Instrux.Services.Implementations;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IRepository _repo;

    public AuthenticationService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var teacher = await _repo.FirstOrDefaultAsync<Teacher>(item => item.Email.ToLower() == email);
        if (teacher is null || teacher.PasswordHash != request.Password)
        {
            return new AuthResultDto(false, "Invalid email or password.", null);
        }

        return new AuthResultDto(true, "Signed in.", DtoMapper.ToDto(teacher));
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _repo.AnyAsync<Teacher>(item => item.Email.ToLower() == email))
        {
            return new AuthResultDto(false, "That email is already registered.", null);
        }

        var teacher = new Teacher
        {
            FullName = request.FullName.Trim(),
            Nickname = request.Nickname.Trim(),
            Email = email,
            PasswordHash = request.Password
        };

        _repo.Add(teacher);
        await _repo.SaveChangesAsync();

        return new AuthResultDto(true, "Account created.", DtoMapper.ToDto(teacher));
    }
}
