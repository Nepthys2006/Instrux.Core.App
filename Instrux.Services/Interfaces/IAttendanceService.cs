using Instrux.Domain.Enums;
using Instrux.Services.DTOs;

namespace Instrux.Services.Interfaces;

public interface IAttendanceService
{
    Task<List<AttendanceRecordDto>> GetAllAsync(int teacherId);
    Task<List<AttendanceRecordDto>> GetByDateAsync(int classId, DateTime date);
    Task<AttendanceRecordDto> SaveRecordAsync(int studentId, DateTime date, AttendanceStatus status, string? note = null);
    Task SaveBatchAsync(AttendanceBatchDto batch);
}
