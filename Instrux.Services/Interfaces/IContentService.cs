using Instrux.Services.DTOs;

namespace Instrux.Services.Interfaces;

public interface IContentService
{
    Task<List<ContentItemDto>> GetAllAsync(int teacherId);
    Task<List<ContentItemDto>> GetByClassAsync(int classId);
    Task<ContentItemDto> CreateAsync(CreateContentItemDto request);
    Task DeleteAsync(int id);
}
