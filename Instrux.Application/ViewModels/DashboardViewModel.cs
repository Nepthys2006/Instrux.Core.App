using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Windows.Threading;
using Instrux.Application.Helpers;
using Instrux.Application.Services;
using Instrux.Domain.Models;

namespace Instrux.Application.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private readonly DataService _dataService;
    private readonly DispatcherTimer _refreshTimer;

    public DashboardViewModel(DataService dataService, SessionService sessionService)
    {
        _dataService = dataService;
        Greeting = $"Good day, {sessionService.CurrentTeacher.Nickname}";
        Today = DateTime.Today.ToString("dddd, MMMM d");
        Classes = dataService.Classes;
        TodayEvents = [];
        FocusTodos = [];
        OpenClassCommand = new RelayCommand(parameter =>
        {
            if (parameter is Class classItem)
            {
                OpenClassRequested?.Invoke(this, classItem);
            }
        });

        dataService.Classes.CollectionChanged += OnDataChanged;
        dataService.Students.CollectionChanged += OnDataChanged;
        dataService.Attendance.CollectionChanged += OnDataChanged;
        dataService.Events.CollectionChanged += OnDataChanged;
        dataService.Todos.CollectionChanged += OnDataChanged;
        Refresh();

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _refreshTimer.Tick += (_, _) => Refresh();
        _refreshTimer.Start();
    }

    public event EventHandler<Class>? OpenClassRequested;

    public string Greeting { get; }
    public string Today { get; }
    public ObservableCollection<Class> Classes { get; }
    public ObservableCollection<CalendarEvent> TodayEvents { get; }
    public ObservableCollection<TodoItem> FocusTodos { get; }
    public ICommand OpenClassCommand { get; }

    public int ClassCount => _dataService.Classes.Count;
    public int StudentCount => _dataService.Students.Count;
    public int TasksDueToday => _dataService.Todos.Count(item => item.DueDate?.Date == DateTime.Today && !item.IsCompleted);
    public int AttendanceMarkedToday => _dataService.Attendance.Count(item => item.Date.Date == DateTime.Today);

    private void OnDataChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    private void Refresh()
    {
        Replace(TodayEvents, _dataService.Events.Where(item => item.Date.Date >= DateTime.Today).OrderBy(item => item.Date).Take(4));
        Replace(FocusTodos, _dataService.Todos.Where(item => !item.IsCompleted).OrderBy(item => item.DueDate ?? DateTime.MaxValue).Take(5));
        OnPropertyChanged(nameof(ClassCount));
        OnPropertyChanged(nameof(StudentCount));
        OnPropertyChanged(nameof(TasksDueToday));
        OnPropertyChanged(nameof(AttendanceMarkedToday));
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
