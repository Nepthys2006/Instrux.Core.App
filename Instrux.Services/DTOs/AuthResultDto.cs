namespace Instrux.Services.DTOs;

public sealed record AuthResultDto(bool Success, string Message, TeacherDto? Teacher);
