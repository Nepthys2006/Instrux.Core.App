using Instrux.Services.DTOs;

namespace Instrux.Services.Interfaces;

public interface IStudentService
{
    Task<List<StudentDto>> GetAllAsync(int teacherId);
    Task<List<StudentDto>> GetByClassAsync(int classId);
    Task<StudentDto> CreateAsync(CreateStudentDto request);
    Task DeleteAsync(int id);
}
