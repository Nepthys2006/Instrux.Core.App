using Instrux.Domain.Enums;

namespace Instrux.Services.DTOs;

public sealed record ClassDto(int Id, string Name, string? Section, Subject Subject, string? SchoolYear, string? Semester, string CoverColor, int TeacherId, int StudentCount);
