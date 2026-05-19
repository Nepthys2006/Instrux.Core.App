using Instrux.Services.DTOs;

namespace Instrux.Services.Interfaces;

public interface ICalendarEventService
{
    Task<List<CalendarEventDto>> GetAllAsync(int teacherId);
    Task<List<CalendarEventDto>> GetByMonthAsync(int teacherId, int year, int month);
    Task<List<CalendarEventDto>> GetTodayAsync(int teacherId);
    Task<CalendarEventDto> CreateAsync(CreateEventDto request);
    Task DeleteAsync(int id);
}
