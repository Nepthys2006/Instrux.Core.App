using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class CalendarEventService : ICalendarEventService
{
    private readonly InstruxDbContext _dbContext;

    public CalendarEventService(InstruxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CalendarEventDto>> GetAllAsync(int teacherId) => (await _dbContext.CalendarEvents.Where(item => item.TeacherId == teacherId).OrderBy(item => item.Date).ToListAsync()).Select(DtoMapper.ToDto).ToList();

    public async Task<List<CalendarEventDto>> GetByMonthAsync(int teacherId, int year, int month) => (await _dbContext.CalendarEvents.Where(item => item.TeacherId == teacherId && item.Date.Year == year && item.Date.Month == month).OrderBy(item => item.Date).ToListAsync()).Select(DtoMapper.ToDto).ToList();

    public async Task<List<CalendarEventDto>> GetTodayAsync(int teacherId) => (await _dbContext.CalendarEvents.Where(item => item.TeacherId == teacherId && item.Date == DateTime.Today).OrderBy(item => item.StartTime).ToListAsync()).Select(DtoMapper.ToDto).ToList();

    public async Task<CalendarEventDto> CreateAsync(CreateEventDto request)
    {
        var calendarEvent = DtoMapper.ToEntity(request);
        _dbContext.CalendarEvents.Add(calendarEvent);
        await _dbContext.SaveChangesAsync();
        return DtoMapper.ToDto(calendarEvent);
    }

    public async Task DeleteAsync(int id)
    {
        var calendarEvent = await _dbContext.CalendarEvents.FindAsync(id);
        if (calendarEvent is null)
        {
            return;
        }

        _dbContext.CalendarEvents.Remove(calendarEvent);
        await _dbContext.SaveChangesAsync();
    }
}
