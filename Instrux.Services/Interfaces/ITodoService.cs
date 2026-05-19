using Instrux.Services.DTOs;

namespace Instrux.Services.Interfaces;

public interface ITodoService
{
    Task<List<TodoItemDto>> GetAllAsync(int teacherId);
    Task<TodoItemDto> CreateAsync(CreateTodoDto request);
    Task<TodoItemDto> ToggleAsync(int id);
    Task DeleteAsync(int id);
}
