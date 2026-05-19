using Instrux.Domain.Models;
using Instrux.Infrastructure.Repositories;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class ClassService : IClassService
{
    private readonly IRepository _repo;

    public ClassService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ClassDto>> GetAllAsync(int teacherId)
    {
        var classes = await _repo.FindAsync<Class>(item => item.TeacherId == teacherId);
        var ordered = classes.OrderBy(item => item.Name).ToList();
        var counts = await _repo.Query<Student>().GroupBy(item => item.ClassId).Select(group => new { ClassId = group.Key, Count = group.Count() }).ToDictionaryAsync(item => item.ClassId, item => item.Count);
        return ordered.Select(item => DtoMapper.ToDto(item, counts.GetValueOrDefault(item.Id))).ToList();
    }

    public async Task<ClassDto?> GetByIdAsync(int id)
    {
        var classItem = await _repo.GetByIdAsync<Class>(id);
        return classItem is null ? null : DtoMapper.ToDto(classItem, await _repo.CountAsync<Student>(student => student.ClassId == id));
    }

    public async Task<ClassDto> CreateAsync(CreateClassDto request)
    {
        var classItem = DtoMapper.ToEntity(request);
        _repo.Add(classItem);
        await _repo.SaveChangesAsync();
        return DtoMapper.ToDto(classItem);
    }

    public async Task DeleteAsync(int id)
    {
        var classItem = await _repo.GetByIdAsync<Class>(id);
        if (classItem is null)
        {
            return;
        }

        var studentIds = await _repo.FindAsync<Student>(student => student.ClassId == id).ContinueWith(t => t.Result.Select(s => s.Id).ToList());
        var assessmentIds = await _repo.FindAsync<Assessment>(assessment => assessment.ClassId == id).ContinueWith(t => t.Result.Select(a => a.Id).ToList());

        var scores = await _repo.FindAsync<Score>(score => studentIds.Contains(score.StudentId) || assessmentIds.Contains(score.AssessmentId));
        var attendance = await _repo.FindAsync<AttendanceRecord>(record => studentIds.Contains(record.StudentId));
        var students = await _repo.FindAsync<Student>(student => student.ClassId == id);
        var assessments = await _repo.FindAsync<Assessment>(assessment => assessment.ClassId == id);
        var contentItems = await _repo.FindAsync<ContentItem>(content => content.ClassId == id);
        var linkedEvents = await _repo.FindAsync<CalendarEvent>(calendarEvent => calendarEvent.LinkedClassId == id);
        var linkedTodos = await _repo.FindAsync<TodoItem>(todo => todo.LinkedClassId == id);

        foreach (var calendarEvent in linkedEvents)
        {
            calendarEvent.LinkedClassId = null;
        }

        foreach (var todo in linkedTodos)
        {
            todo.LinkedClassId = null;
        }

        _repo.DeleteRange(scores);
        _repo.DeleteRange(attendance);
        _repo.DeleteRange(students);
        _repo.DeleteRange(assessments);
        _repo.DeleteRange(contentItems);
        _repo.Delete(classItem);
        await _repo.SaveChangesAsync();
    }
}
