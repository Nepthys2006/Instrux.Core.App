using Instrux.Domain.Models;
using Instrux.Infrastructure.Repositories;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;

namespace Instrux.Services.Implementations;

public sealed class CalendarEventService : ICalendarEventService
{
    private readonly IRepository _repo;

    public CalendarEventService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<CalendarEventDto>> GetAllAsync(int teacherId) => (await _repo.FindAsync<CalendarEvent>(item => item.TeacherId == teacherId))
        .OrderBy(item => item.Date)
        .Select(DtoMapper.ToDto)
        .ToList();

    public async Task<List<CalendarEventDto>> GetByMonthAsync(int teacherId, int year, int month)
    {
        var events = await _repo.FindAsync<CalendarEvent>(item => item.TeacherId == teacherId && item.Date.Year == year && item.Date.Month == month);
        return events.OrderBy(item => item.Date).Select(DtoMapper.ToDto).ToList();
    }

    public async Task<List<CalendarEventDto>> GetTodayAsync(int teacherId)
    {
        var events = await _repo.FindAsync<CalendarEvent>(item => item.TeacherId == teacherId && item.Date == DateTime.Today);
        return events.OrderBy(item => item.StartTime).Select(DtoMapper.ToDto).ToList();
    }

    public async Task<CalendarEventDto> CreateAsync(CreateEventDto request)
    {
        var calendarEvent = DtoMapper.ToEntity(request);
        _repo.Add(calendarEvent);
        await _repo.SaveChangesAsync();
        return DtoMapper.ToDto(calendarEvent);
    }

    public async Task DeleteAsync(int id)
    {
        var calendarEvent = await _repo.GetByIdAsync<CalendarEvent>(id);
        if (calendarEvent is null)
        {
            return;
        }

        _repo.Delete(calendarEvent);
        await _repo.SaveChangesAsync();
    }
}
