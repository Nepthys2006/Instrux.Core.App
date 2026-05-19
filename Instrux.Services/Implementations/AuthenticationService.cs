using Instrux.Domain.Models;
using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly InstruxDbContext _dbContext;

    public AuthenticationService(InstruxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var teacher = await _dbContext.Teachers.FirstOrDefaultAsync(item => item.Email.ToLower() == email);
        if (teacher is null || teacher.PasswordHash != request.Password)
        {
            return new AuthResultDto(false, "Invalid email or password.", null);
        }

        return new AuthResultDto(true, "Signed in.", DtoMapper.ToDto(teacher));
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _dbContext.Teachers.AnyAsync(item => item.Email.ToLower() == email))
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

        _dbContext.Teachers.Add(teacher);
        await _dbContext.SaveChangesAsync();

        return new AuthResultDto(true, "Account created.", DtoMapper.ToDto(teacher));
    }
}
