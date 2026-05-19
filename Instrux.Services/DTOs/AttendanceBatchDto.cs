namespace Instrux.Services.DTOs;

public sealed record AttendanceBatchDto(int ClassId, DateTime Date, IReadOnlyList<AttendanceRecordDto> Records);
