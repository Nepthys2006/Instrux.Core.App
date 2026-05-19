using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class ClassService : IClassService
{
    private readonly InstruxDbContext _dbContext;

    public ClassService(InstruxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ClassDto>> GetAllAsync(int teacherId)
    {
        var classes = await _dbContext.Classes.Where(item => item.TeacherId == teacherId).OrderBy(item => item.Name).ToListAsync();
        var counts = await _dbContext.Students.GroupBy(item => item.ClassId).Select(group => new { ClassId = group.Key, Count = group.Count() }).ToDictionaryAsync(item => item.ClassId, item => item.Count);
        return classes.Select(item => DtoMapper.ToDto(item, counts.GetValueOrDefault(item.Id))).ToList();
    }

    public async Task<ClassDto?> GetByIdAsync(int id)
    {
        var classItem = await _dbContext.Classes.FindAsync(id);
        return classItem is null ? null : DtoMapper.ToDto(classItem, await _dbContext.Students.CountAsync(student => student.ClassId == id));
    }

    public async Task<ClassDto> CreateAsync(CreateClassDto request)
    {
        var classItem = DtoMapper.ToEntity(request);
        _dbContext.Classes.Add(classItem);
        await _dbContext.SaveChangesAsync();
        return DtoMapper.ToDto(classItem);
    }

    public async Task DeleteAsync(int id)
    {
        var classItem = await _dbContext.Classes.FindAsync(id);
        if (classItem is null)
        {
            return;
        }

        var studentIds = await _dbContext.Students.Where(student => student.ClassId == id).Select(student => student.Id).ToListAsync();
        var assessmentIds = await _dbContext.Assessments.Where(assessment => assessment.ClassId == id).Select(assessment => assessment.Id).ToListAsync();

        var scores = await _dbContext.Scores
            .Where(score => studentIds.Contains(score.StudentId) || assessmentIds.Contains(score.AssessmentId))
            .ToListAsync();
        var attendance = await _dbContext.AttendanceRecords.Where(record => studentIds.Contains(record.StudentId)).ToListAsync();
        var students = await _dbContext.Students.Where(student => student.ClassId == id).ToListAsync();
        var assessments = await _dbContext.Assessments.Where(assessment => assessment.ClassId == id).ToListAsync();
        var contentItems = await _dbContext.ContentItems.Where(content => content.ClassId == id).ToListAsync();
        var linkedEvents = await _dbContext.CalendarEvents.Where(calendarEvent => calendarEvent.LinkedClassId == id).ToListAsync();
        var linkedTodos = await _dbContext.TodoItems.Where(todo => todo.LinkedClassId == id).ToListAsync();

        foreach (var calendarEvent in linkedEvents)
        {
            calendarEvent.LinkedClassId = null;
        }

        foreach (var todo in linkedTodos)
        {
            todo.LinkedClassId = null;
        }

        _dbContext.Scores.RemoveRange(scores);
        _dbContext.AttendanceRecords.RemoveRange(attendance);
        _dbContext.Students.RemoveRange(students);
        _dbContext.Assessments.RemoveRange(assessments);
        _dbContext.ContentItems.RemoveRange(contentItems);
        _dbContext.Classes.Remove(classItem);
        await _dbContext.SaveChangesAsync();
    }
}
