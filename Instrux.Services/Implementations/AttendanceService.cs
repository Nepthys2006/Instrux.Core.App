using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Infrastructure.Repositories;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class AttendanceService : IAttendanceService
{
    private readonly IRepository _repo;

    public AttendanceService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<AttendanceRecordDto>> GetAllAsync(int teacherId) => (await _repo.Query<AttendanceRecord>()
        .Where(record => _repo.Query<Student>().Any(student => student.Id == record.StudentId
            && _repo.Query<Class>().Any(classItem => classItem.Id == student.ClassId && classItem.TeacherId == teacherId)))
        .ToListAsync())
        .Select(record => DtoMapper.ToDto(record))
        .ToList();

    public async Task<List<AttendanceRecordDto>> GetByDateAsync(int classId, DateTime date)
    {
        var studentIds = await _repo.FindAsync<Student>(student => student.ClassId == classId).ContinueWith(t => t.Result.Select(s => s.Id).ToList());
        var records = await _repo.FindAsync<AttendanceRecord>(record => studentIds.Contains(record.StudentId) && record.Date == date.Date);
        var names = await _repo.Query<Student>().Where(student => studentIds.Contains(student.Id)).ToDictionaryAsync(student => student.Id, student => student.FullName);
        return records.Select(record => DtoMapper.ToDto(record, names.GetValueOrDefault(record.StudentId))).ToList();
    }

    public async Task<AttendanceRecordDto> SaveRecordAsync(int studentId, DateTime date, AttendanceStatus status, string? note = null)
    {
        var record = await _repo.FirstOrDefaultAsync<AttendanceRecord>(item => item.StudentId == studentId && item.Date == date.Date);
        if (record is null)
        {
            record = new AttendanceRecord { StudentId = studentId, Date = date.Date };
            _repo.Add(record);
        }

        record.Status = status;
        record.Note = note;
        await _repo.SaveChangesAsync();
        return DtoMapper.ToDto(record);
    }

    public async Task SaveBatchAsync(AttendanceBatchDto batch)
    {
        foreach (var record in batch.Records)
        {
            await SaveRecordAsync(record.StudentId, batch.Date, record.Status, record.Note);
        }
    }
}
