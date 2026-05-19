using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class TodoService : ITodoService
{
    private readonly InstruxDbContext _dbContext;

    public TodoService(InstruxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TodoItemDto>> GetAllAsync(int teacherId) => (await _dbContext.TodoItems.Where(item => item.TeacherId == teacherId).OrderBy(item => item.IsCompleted).ThenBy(item => item.DueDate).ToListAsync()).Select(DtoMapper.ToDto).ToList();

    public async Task<TodoItemDto> CreateAsync(CreateTodoDto request)
    {
        var todo = DtoMapper.ToEntity(request);
        _dbContext.TodoItems.Add(todo);
        await _dbContext.SaveChangesAsync();
        return DtoMapper.ToDto(todo);
    }

    public async Task<TodoItemDto> ToggleAsync(int id)
    {
        var todo = await _dbContext.TodoItems.FindAsync(id) ?? throw new InvalidOperationException("Task not found.");
        todo.IsCompleted = !todo.IsCompleted;
        todo.CompletedAt = todo.IsCompleted ? DateTime.Now : null;
        await _dbContext.SaveChangesAsync();
        return DtoMapper.ToDto(todo);
    }

    public async Task DeleteAsync(int id)
    {
        var todo = await _dbContext.TodoItems.FindAsync(id);
        if (todo is null)
        {
            return;
        }

        _dbContext.TodoItems.Remove(todo);
        await _dbContext.SaveChangesAsync();
    }
}
