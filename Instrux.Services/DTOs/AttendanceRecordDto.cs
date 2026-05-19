using Instrux.Domain.Enums;

namespace Instrux.Services.DTOs;

public sealed record AttendanceRecordDto(int Id, int StudentId, string? StudentName, DateTime Date, AttendanceStatus Status, string? Note);
