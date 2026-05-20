using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Instrux.Application.Helpers;
using Instrux.Application.Services;
using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Services.Exceptions;

namespace Instrux.Application.ViewModels;

public sealed class CalendarViewModel : ViewModelBase
{
    private readonly DataService _dataService;
    private readonly NotificationService _notifications;
    private DateTime _visibleMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime _selectedDate = DateTime.Today;
    private string _newEventTitle = string.Empty;
    private EventCategory _selectedCategory = EventCategory.Reminder;
    private string _startTimeText = string.Empty;
    private string _eventNotes = string.Empty;

    public CalendarViewModel(DataService dataService, NotificationService notificationService)
    {
        _dataService = dataService;
        _notifications = notificationService;
        Categories = Enum.GetValues<EventCategory>();
        PreviousMonthCommand = new RelayCommand(() => VisibleMonth = VisibleMonth.AddMonths(-1), onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        NextMonthCommand = new RelayCommand(() => VisibleMonth = VisibleMonth.AddMonths(1), onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        SelectDayCommand = new RelayCommand(SelectDay, onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        AddEventCommand = new RelayCommandAsync(AddEventAsync, () => !string.IsNullOrWhiteSpace(NewEventTitle), ex => _notifications.ShowError(UnwrapMessage(ex)));
        DeleteEventCommand = new RelayCommand(DeleteEvent, onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        _dataService.Events.CollectionChanged += OnEventsChanged;
        BuildMonth();
        RefreshAgenda();
    }

    public ObservableCollection<CalendarDayViewModel> Days { get; } = [];
    public ObservableCollection<CalendarEvent> TodayAgenda { get; } = [];
    public ObservableCollection<CalendarEvent> UpcomingEvents { get; } = [];
    public IReadOnlyList<EventCategory> Categories { get; }
    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand SelectDayCommand { get; }
    public ICommand AddEventCommand { get; }
    public ICommand DeleteEventCommand { get; }

    public DateTime VisibleMonth
    {
        get => _visibleMonth;
        set
        {
            if (SetProperty(ref _visibleMonth, value))
            {
                OnPropertyChanged(nameof(MonthTitle));
                BuildMonth();
            }
        }
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                OnPropertyChanged(nameof(SelectedDateTitle));
                RefreshAgenda();
            }
        }
    }

    public string NewEventTitle
    {
        get => _newEventTitle;
        set
        {
            if (SetProperty(ref _newEventTitle, value))
            {
                (AddEventCommand as RelayCommandAsync)?.RaiseCanExecuteChanged();
            }
        }
    }

    public EventCategory SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public string StartTimeText
    {
        get => _startTimeText;
        set => SetProperty(ref _startTimeText, value);
    }

    public string EventNotes
    {
        get => _eventNotes;
        set => SetProperty(ref _eventNotes, value);
    }

    public string MonthTitle => VisibleMonth.ToString("MMMM yyyy");
    public string SelectedDateTitle => SelectedDate.ToString("dddd, MMMM d");

    private void SelectDay(object? parameter)
    {
        if (parameter is not CalendarDayViewModel day)
        {
            return;
        }

        SelectedDate = day.Date.Date;
        if (day.Date.Month != VisibleMonth.Month)
        {
            VisibleMonth = new DateTime(day.Date.Year, day.Date.Month, 1);
        }
    }

    private async Task AddEventAsync()
    {
        TimeSpan? startTime = null;

        if (!string.IsNullOrWhiteSpace(StartTimeText))
        {
            var trimmed = StartTimeText.Trim().ToUpperInvariant();
            if (!Regex.IsMatch(trimmed, @"^((1[0-2]|0?[1-9]):[0-5]\d\s?(AM|PM)|([01]\d|2[0-3]):[0-5]\d)$"))
            {
                ErrorMessage = "Start time must be in HH:mm or h:mm AM/PM format (e.g., 14:30 or 2:30 PM).";
                return;
            }

            TimeSpan? parsedTime = null;
            if (trimmed.EndsWith("AM") || trimmed.EndsWith("PM"))
            {
                if (DateTime.TryParseExact(trimmed, ["h:mm tt", "hh:mm tt"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    parsedTime = dt.TimeOfDay;
                }
            }
            else
            {
                parsedTime = TimeSpan.TryParse(trimmed, out var ts) ? ts : null;
            }

            if (parsedTime is null)
            {
                ErrorMessage = "Could not parse the start time. Use HH:mm or h:mm AM/PM format.";
                return;
            }

            startTime = parsedTime;
        }

        await _dataService.AddEventAsync(new CalendarEvent
        {
            Title = NewEventTitle.Trim(),
            Date = SelectedDate.Date,
            StartTime = startTime,
            Category = SelectedCategory,
            Notes = string.IsNullOrWhiteSpace(EventNotes) ? null : EventNotes.Trim()
        });

        NewEventTitle = string.Empty;
        StartTimeText = string.Empty;
        EventNotes = string.Empty;
        BuildMonth();
        RefreshAgenda();
    }

    private async void DeleteEvent(object? parameter)
    {
        try
        {
            if (parameter is not CalendarEvent calendarEvent)
            {
                return;
            }

            await _dataService.DeleteEventAsync(calendarEvent);
            BuildMonth();
            RefreshAgenda();
        }
        catch (Exception ex)
        {
            _notifications.ShowError(UnwrapMessage(ex));
        }
    }

    private static string UnwrapMessage(Exception ex) => ex is ServiceException se ? se.UserFacingMessage : "Something went wrong. Please try again.";

    private void OnEventsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BuildMonth();
        RefreshAgenda();
    }

    private void BuildMonth()
    {
        Days.Clear();
        var first = new DateTime(VisibleMonth.Year, VisibleMonth.Month, 1);
        var start = first.AddDays(-(int)first.DayOfWeek);

        for (var index = 0; index < 42; index++)
        {
            var date = start.AddDays(index);
            var day = new CalendarDayViewModel
            {
                Date = date,
                IsCurrentMonth = date.Month == VisibleMonth.Month,
                IsSelected = date.Date == SelectedDate.Date
            };

            foreach (var calendarEvent in _dataService.Events.Where(item => item.Date.Date == date.Date).OrderBy(item => item.StartTime))
            {
                day.Events.Add(calendarEvent);
            }

            Days.Add(day);
        }
    }

    private void RefreshAgenda()
    {
        Replace(TodayAgenda, _dataService.Events.Where(item => item.Date.Date == SelectedDate.Date).OrderBy(item => item.StartTime));
        Replace(UpcomingEvents, _dataService.Events.Where(item => item.Date.Date >= DateTime.Today).OrderBy(item => item.Date).ThenBy(item => item.StartTime).Take(8));
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }
}
