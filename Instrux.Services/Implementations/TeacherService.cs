using Instrux.Domain.Models;
using Instrux.Infrastructure.Repositories;
using Instrux.Services.DTOs;
using Instrux.Services.Exceptions;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class TeacherService : ITeacherService
{
    private readonly IRepository _repo;

    public TeacherService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<TeacherDto?> GetProfileAsync(int teacherId)
    {
        var teacher = await _repo.GetByIdAsync<Teacher>(teacherId);
        return teacher is null ? null : DtoMapper.ToDto(teacher);
    }

    public async Task<TeacherDto> UpdateProfileAsync(TeacherDto teacher)
    {
        var entity = await _repo.GetByIdAsync<Teacher>(teacher.Id) ?? throw new ServiceException("Teacher not found.");
        entity.FullName = teacher.FullName;
        entity.Nickname = teacher.Nickname;
        entity.Email = teacher.Email;
        await _repo.SaveChangesAsync();
        return DtoMapper.ToDto(entity);
    }

    public async Task DeleteAccountAsync(int teacherId)
    {
        var classIds = await _repo.FindAsync<Class>(c => c.TeacherId == teacherId).ContinueWith(t => t.Result.Select(c => c.Id).ToList());
        var studentIds = await _repo.FindAsync<Student>(s => classIds.Contains(s.ClassId)).ContinueWith(t => t.Result.Select(s => s.Id).ToList());
        var assessmentIds = await _repo.FindAsync<Assessment>(a => classIds.Contains(a.ClassId)).ContinueWith(t => t.Result.Select(a => a.Id).ToList());

        var scores = await _repo.FindAsync<Score>(s => studentIds.Contains(s.StudentId) || assessmentIds.Contains(s.AssessmentId));
        _repo.DeleteRange(scores);

        var attendance = await _repo.FindAsync<AttendanceRecord>(a => studentIds.Contains(a.StudentId));
        _repo.DeleteRange(attendance);

        _repo.DeleteRange(await _repo.FindAsync<Student>(s => classIds.Contains(s.ClassId)));
        _repo.DeleteRange(await _repo.FindAsync<Assessment>(a => classIds.Contains(a.ClassId)));
        _repo.DeleteRange(await _repo.FindAsync<ContentItem>(c => classIds.Contains(c.ClassId)));
        _repo.DeleteRange(await _repo.FindAsync<Class>(c => c.TeacherId == teacherId));
        _repo.DeleteRange(await _repo.FindAsync<CalendarEvent>(e => e.TeacherId == teacherId));
        _repo.DeleteRange(await _repo.FindAsync<TodoItem>(t => t.TeacherId == teacherId));

        var teacher = await _repo.GetByIdAsync<Teacher>(teacherId);
        if (teacher is not null)
        {
            _repo.Delete(teacher);
        }

        await _repo.SaveChangesAsync();
    }
}
