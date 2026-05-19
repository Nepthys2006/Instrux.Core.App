using Instrux.Domain.Enums;

namespace Instrux.Services.DTOs;

public sealed record CreateClassDto(string Name, string? Section, Subject Subject, string? SchoolYear, string? Semester, string CoverColor, int TeacherId);
