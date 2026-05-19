using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class ContentService : IContentService
{
    private readonly InstruxDbContext _dbContext;

    public ContentService(InstruxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ContentItemDto>> GetAllAsync(int teacherId) => (await _dbContext.ContentItems
        .Where(item => _dbContext.Classes.Any(classItem => classItem.Id == item.ClassId && classItem.TeacherId == teacherId))
        .OrderByDescending(item => item.UploadedAt)
        .ToListAsync())
        .Select(DtoMapper.ToDto)
        .ToList();

    public async Task<List<ContentItemDto>> GetByClassAsync(int classId) => (await _dbContext.ContentItems.Where(item => item.ClassId == classId).OrderByDescending(item => item.UploadedAt).ToListAsync()).Select(DtoMapper.ToDto).ToList();

    public async Task<ContentItemDto> CreateAsync(CreateContentItemDto request)
    {
        var content = DtoMapper.ToEntity(request);
        _dbContext.ContentItems.Add(content);
        await _dbContext.SaveChangesAsync();
        return DtoMapper.ToDto(content);
    }

    public async Task DeleteAsync(int id)
    {
        var content = await _dbContext.ContentItems.FindAsync(id);
        if (content is null)
        {
            return;
        }

        _dbContext.ContentItems.Remove(content);
        await _dbContext.SaveChangesAsync();
    }
}
