namespace Instrux.Services.DTOs;

public sealed record CreateStudentDto(string FullName, string StudentId, string? Email, int ClassId);
