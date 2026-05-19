namespace Instrux.Services.DTOs;

public sealed record StudentDto(int Id, string FullName, string StudentId, string? Email, int ClassId);
