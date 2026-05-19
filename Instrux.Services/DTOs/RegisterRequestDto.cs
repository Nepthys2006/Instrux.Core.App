namespace Instrux.Services.DTOs;

public sealed record RegisterRequestDto(string FullName, string Nickname, string Email, string Password);
