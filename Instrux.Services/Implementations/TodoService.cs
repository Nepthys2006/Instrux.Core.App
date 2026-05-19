using Instrux.Domain.Models;
using Instrux.Infrastructure.Repositories;
using Instrux.Services.DTOs;
using Instrux.Services.Exceptions;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;

namespace Instrux.Services.Implementations;

public sealed class TodoService : ITodoService
{
    private readonly IRepository _repo;

    public TodoService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<TodoItemDto>> GetAllAsync(int teacherId)
    {
        var items = await _repo.FindAsync<TodoItem>(item => item.TeacherId == teacherId);
        return items.OrderBy(item => item.IsCompleted).ThenBy(item => item.DueDate).Select(DtoMapper.ToDto).ToList();
    }

    public async Task<TodoItemDto> CreateAsync(CreateTodoDto request)
    {
        var todo = DtoMapper.ToEntity(request);
        _repo.Add(todo);
        await _repo.SaveChangesAsync();
        return DtoMapper.ToDto(todo);
    }

    public async Task<TodoItemDto> ToggleAsync(int id)
    {
        var todo = await _repo.GetByIdAsync<TodoItem>(id) ?? throw new ServiceException("Task not found.");
        todo.IsCompleted = !todo.IsCompleted;
        todo.CompletedAt = todo.IsCompleted ? DateTime.Now : null;
        await _repo.SaveChangesAsync();
        return DtoMapper.ToDto(todo);
    }

    public async Task DeleteAsync(int id)
    {
        var todo = await _repo.GetByIdAsync<TodoItem>(id);
        if (todo is null)
        {
            return;
        }

        _repo.Delete(todo);
        await _repo.SaveChangesAsync();
    }
}
