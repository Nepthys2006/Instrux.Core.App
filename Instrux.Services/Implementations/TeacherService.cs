using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class TeacherService : ITeacherService
{
    private readonly InstruxDbContext _dbContext;

    public TeacherService(InstruxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TeacherDto?> GetProfileAsync(int teacherId)
    {
        var teacher = await _dbContext.Teachers.FindAsync(teacherId);
        return teacher is null ? null : DtoMapper.ToDto(teacher);
    }

    public async Task<TeacherDto> UpdateProfileAsync(TeacherDto teacher)
    {
        var entity = await _dbContext.Teachers.FindAsync(teacher.Id) ?? throw new InvalidOperationException("Teacher not found.");
        entity.FullName = teacher.FullName;
        entity.Nickname = teacher.Nickname;
        entity.Email = teacher.Email;
        await _dbContext.SaveChangesAsync();
        return DtoMapper.ToDto(entity);
    }

    public async Task DeleteAccountAsync(int teacherId)
    {
        var classIds = await _dbContext.Classes.Where(c => c.TeacherId == teacherId).Select(c => c.Id).ToListAsync();
        var studentIds = await _dbContext.Students.Where(s => classIds.Contains(s.ClassId)).Select(s => s.Id).ToListAsync();
        var assessmentIds = await _dbContext.Assessments.Where(a => classIds.Contains(a.ClassId)).Select(a => a.Id).ToListAsync();

        var scores = await _dbContext.Scores.Where(s => studentIds.Contains(s.StudentId) || assessmentIds.Contains(s.AssessmentId)).ToListAsync();
        _dbContext.Scores.RemoveRange(scores);

        var attendance = await _dbContext.AttendanceRecords.Where(a => studentIds.Contains(a.StudentId)).ToListAsync();
        _dbContext.AttendanceRecords.RemoveRange(attendance);

        _dbContext.Students.RemoveRange(await _dbContext.Students.Where(s => classIds.Contains(s.ClassId)).ToListAsync());
        _dbContext.Assessments.RemoveRange(await _dbContext.Assessments.Where(a => classIds.Contains(a.ClassId)).ToListAsync());
        _dbContext.ContentItems.RemoveRange(await _dbContext.ContentItems.Where(c => classIds.Contains(c.ClassId)).ToListAsync());
        _dbContext.Classes.RemoveRange(await _dbContext.Classes.Where(c => c.TeacherId == teacherId).ToListAsync());
        _dbContext.CalendarEvents.RemoveRange(await _dbContext.CalendarEvents.Where(e => e.TeacherId == teacherId).ToListAsync());
        _dbContext.TodoItems.RemoveRange(await _dbContext.TodoItems.Where(t => t.TeacherId == teacherId).ToListAsync());

        var teacher = await _dbContext.Teachers.FindAsync(teacherId);
        if (teacher is not null)
        {
            _dbContext.Teachers.Remove(teacher);
        }

        await _dbContext.SaveChangesAsync();
    }
}
