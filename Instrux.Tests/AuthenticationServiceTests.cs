using Instrux.Infrastructure.Data;
using Instrux.Infrastructure.Repositories;
using Instrux.Services.DTOs;
using Instrux.Services.Implementations;

namespace Instrux.Tests;

public sealed class AuthenticationServiceTests : IDisposable
{
    private readonly InstruxDbContext _context;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _service = new AuthenticationService(new Repository(_context));
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Register_CreatesTeacher_ReturnsSuccess()
    {
        var request = new RegisterRequestDto("Jane Doe", "Jane", "jane@test.com", "pass123");

        var result = await _service.RegisterAsync(request);

        Assert.True(result.Success);
        Assert.Equal("Account created.", result.Message);
        Assert.NotNull(result.Teacher);
        Assert.Equal("Jane Doe", result.Teacher.FullName);
        Assert.Equal("Jane", result.Teacher.Nickname);
        Assert.Equal("jane@test.com", result.Teacher.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsFailure()
    {
        var request = new RegisterRequestDto("Jane Doe", "Jane", "jane@test.com", "pass123");
        await _service.RegisterAsync(request);

        var duplicate = new RegisterRequestDto("Jane 2", "J", "jane@test.com", "other");
        var result = await _service.RegisterAsync(duplicate);

        Assert.False(result.Success);
        Assert.Equal("That email is already registered.", result.Message);
        Assert.Null(result.Teacher);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccess()
    {
        await _service.RegisterAsync(new RegisterRequestDto("Jane Doe", "Jane", "jane@test.com", "pass123"));

        var result = await _service.LoginAsync(new LoginRequestDto("jane@test.com", "pass123"));

        Assert.True(result.Success);
        Assert.Equal("Signed in.", result.Message);
        Assert.NotNull(result.Teacher);
    }

    [Fact]
    public async Task Login_InvalidEmail_ReturnsFailure()
    {
        var result = await _service.LoginAsync(new LoginRequestDto("unknown@test.com", "pass123"));

        Assert.False(result.Success);
        Assert.Equal("Invalid email or password.", result.Message);
        Assert.Null(result.Teacher);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsFailure()
    {
        await _service.RegisterAsync(new RegisterRequestDto("Jane Doe", "Jane", "jane@test.com", "pass123"));

        var result = await _service.LoginAsync(new LoginRequestDto("jane@test.com", "wrongpassword"));

        Assert.False(result.Success);
        Assert.Equal("Invalid email or password.", result.Message);
        Assert.Null(result.Teacher);
    }

    [Fact]
    public async Task Register_EmailIsCaseInsensitive()
    {
        await _service.RegisterAsync(new RegisterRequestDto("Jane Doe", "Jane", "JANE@TEST.COM", "pass123"));

        var result = await _service.LoginAsync(new LoginRequestDto("jane@test.com", "pass123"));

        Assert.True(result.Success);
    }
}
