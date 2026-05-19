using Instrux.Services.DTOs;

namespace Instrux.Services.Interfaces;

public interface IClassService
{
    Task<List<ClassDto>> GetAllAsync(int teacherId);
    Task<ClassDto?> GetByIdAsync(int id);
    Task<ClassDto> CreateAsync(CreateClassDto request);
    Task DeleteAsync(int id);
}
