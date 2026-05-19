using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class StudentService : IStudentService
{
    private readonly InstruxDbContext _dbContext;

    public StudentService(InstruxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<StudentDto>> GetAllAsync(int teacherId) => (await _dbContext.Students
        .Where(item => _dbContext.Classes.Any(classItem => classItem.Id == item.ClassId && classItem.TeacherId == teacherId))
        .OrderBy(item => item.FullName)
        .ToListAsync())
        .Select(DtoMapper.ToDto)
        .ToList();

    public async Task<List<StudentDto>> GetByClassAsync(int classId) => (await _dbContext.Students.Where(item => item.ClassId == classId).OrderBy(item => item.FullName).ToListAsync()).Select(DtoMapper.ToDto).ToList();

    public async Task<StudentDto> CreateAsync(CreateStudentDto request)
    {
        var student = DtoMapper.ToEntity(request);
        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync();
        return DtoMapper.ToDto(student);
    }

    public async Task DeleteAsync(int id)
    {
        var student = await _dbContext.Students.FindAsync(id);
        if (student is null)
        {
            return;
        }

        var attendance = await _dbContext.AttendanceRecords.Where(item => item.StudentId == id).ToListAsync();
        var scores = await _dbContext.Scores.Where(item => item.StudentId == id).ToListAsync();
        _dbContext.AttendanceRecords.RemoveRange(attendance);
        _dbContext.Scores.RemoveRange(scores);
        _dbContext.Students.Remove(student);
        await _dbContext.SaveChangesAsync();
    }
}
