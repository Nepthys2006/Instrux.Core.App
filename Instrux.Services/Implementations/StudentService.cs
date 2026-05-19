using Instrux.Domain.Models;
using Instrux.Infrastructure.Repositories;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class StudentService : IStudentService
{
    private readonly IRepository _repo;

    public StudentService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<StudentDto>> GetAllAsync(int teacherId) => (await _repo.Query<Student>()
        .Where(item => _repo.Query<Class>().Any(classItem => classItem.Id == item.ClassId && classItem.TeacherId == teacherId))
        .OrderBy(item => item.FullName)
        .ToListAsync())
        .Select(DtoMapper.ToDto)
        .ToList();

    public async Task<List<StudentDto>> GetByClassAsync(int classId)
    {
        var items = await _repo.FindAsync<Student>(item => item.ClassId == classId);
        return items.OrderBy(item => item.FullName).Select(DtoMapper.ToDto).ToList();
    }

    public async Task<StudentDto> CreateAsync(CreateStudentDto request)
    {
        var student = DtoMapper.ToEntity(request);
        _repo.Add(student);
        await _repo.SaveChangesAsync();
        return DtoMapper.ToDto(student);
    }

    public async Task DeleteAsync(int id)
    {
        var student = await _repo.GetByIdAsync<Student>(id);
        if (student is null)
        {
            return;
        }

        var attendance = await _repo.FindAsync<AttendanceRecord>(item => item.StudentId == id);
        var scores = await _repo.FindAsync<Score>(item => item.StudentId == id);
        _repo.DeleteRange(attendance);
        _repo.DeleteRange(scores);
        _repo.Delete(student);
        await _repo.SaveChangesAsync();
    }
}
