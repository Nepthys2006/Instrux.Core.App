using Instrux.Services.DTOs;

namespace Instrux.Services.Interfaces;

public interface ITeacherService
{
    Task<TeacherDto?> GetProfileAsync(int teacherId);
    Task<TeacherDto> UpdateProfileAsync(TeacherDto teacher);
    Task DeleteAccountAsync(int teacherId);
}
