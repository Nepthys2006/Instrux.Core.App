using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class AttendanceService : IAttendanceService
{
    private readonly InstruxDbContext _dbContext;

    public AttendanceService(InstruxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AttendanceRecordDto>> GetAllAsync(int teacherId) => (await _dbContext.AttendanceRecords
        .Where(record => _dbContext.Students.Any(student => student.Id == record.StudentId
            && _dbContext.Classes.Any(classItem => classItem.Id == student.ClassId && classItem.TeacherId == teacherId)))
        .ToListAsync())
        .Select(record => DtoMapper.ToDto(record))
        .ToList();

    public async Task<List<AttendanceRecordDto>> GetByDateAsync(int classId, DateTime date)
    {
        var studentIds = await _dbContext.Students.Where(student => student.ClassId == classId).Select(student => student.Id).ToListAsync();
        var records = await _dbContext.AttendanceRecords.Where(record => studentIds.Contains(record.StudentId) && record.Date == date.Date).ToListAsync();
        var names = await _dbContext.Students.Where(student => studentIds.Contains(student.Id)).ToDictionaryAsync(student => student.Id, student => student.FullName);
        return records.Select(record => DtoMapper.ToDto(record, names.GetValueOrDefault(record.StudentId))).ToList();
    }

    public async Task<AttendanceRecordDto> SaveRecordAsync(int studentId, DateTime date, AttendanceStatus status, string? note = null)
    {
        var record = await _dbContext.AttendanceRecords.FirstOrDefaultAsync(item => item.StudentId == studentId && item.Date == date.Date);
        if (record is null)
        {
            record = new AttendanceRecord { StudentId = studentId, Date = date.Date };
            _dbContext.AttendanceRecords.Add(record);
        }

        record.Status = status;
        record.Note = note;
        await _dbContext.SaveChangesAsync();
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
