using Instrux.Domain.Models;
using Instrux.Infrastructure.Repositories;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class ContentService : IContentService
{
    private readonly IRepository _repo;

    public ContentService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ContentItemDto>> GetAllAsync(int teacherId) => (await _repo.Query<ContentItem>()
        .Where(item => _repo.Query<Class>().Any(classItem => classItem.Id == item.ClassId && classItem.TeacherId == teacherId))
        .OrderByDescending(item => item.UploadedAt)
        .ToListAsync())
        .Select(DtoMapper.ToDto)
        .ToList();

    public async Task<List<ContentItemDto>> GetByClassAsync(int classId)
    {
        var items = await _repo.FindAsync<ContentItem>(item => item.ClassId == classId);
        return items.OrderByDescending(item => item.UploadedAt).Select(DtoMapper.ToDto).ToList();
    }

    public async Task<ContentItemDto> CreateAsync(CreateContentItemDto request)
    {
        var content = DtoMapper.ToEntity(request);
        _repo.Add(content);
        await _repo.SaveChangesAsync();
        return DtoMapper.ToDto(content);
    }

    public async Task DeleteAsync(int id)
    {
        var content = await _repo.GetByIdAsync<ContentItem>(id);
        if (content is null)
        {
            return;
        }

        _repo.Delete(content);
        await _repo.SaveChangesAsync();
    }
}
