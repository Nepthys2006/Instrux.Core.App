using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using Instrux.Application.Helpers;
using Instrux.Application.Services;
using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Services.Exceptions;

namespace Instrux.Application.ViewModels;

public sealed class TodoViewModel : ViewModelBase
{
    private readonly DataService _dataService;
    private readonly NotificationService _notifications;
    private string _newTaskTitle = string.Empty;
    private Priority _selectedPriority = Priority.Medium;
    private DateTime? _newTaskDueDate = DateTime.Today;
    private string _selectedFilter = "All";
    private string _todoSearch = string.Empty;

    public TodoViewModel(DataService dataService, NotificationService notificationService)
    {
        _dataService = dataService;
        _notifications = notificationService;
        Todos = dataService.Todos;
        Filters = ["All", "Today", "Upcoming", "Completed"];
        AddTaskCommand = new RelayCommandAsync(AddTaskAsync, () => !string.IsNullOrWhiteSpace(NewTaskTitle), ex => _notifications.ShowError(UnwrapMessage(ex)));
        ToggleTaskCommand = new RelayCommand(ToggleTask, onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        DeleteTaskCommand = new RelayCommand(DeleteTask, onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        SetFilterCommand = new RelayCommand(parameter => SelectedFilter = parameter?.ToString() ?? "All");
        Todos.CollectionChanged += OnTodosChanged;
        RefreshGroups();
    }

    public ObservableCollection<TodoItem> Todos { get; }
    public ObservableCollection<TodoItem> Today { get; } = [];
    public ObservableCollection<TodoItem> Upcoming { get; } = [];
    public ObservableCollection<TodoItem> Completed { get; } = [];
    public IReadOnlyList<Priority> Priorities { get; } = Enum.GetValues<Priority>();
    public IReadOnlyList<string> Filters { get; }
    public ICommand AddTaskCommand { get; }
    public ICommand ToggleTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }
    public ICommand SetFilterCommand { get; }

    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set
        {
            if (SetProperty(ref _newTaskTitle, value))
            {
                (AddTaskCommand as RelayCommandAsync)?.RaiseCanExecuteChanged();
            }
        }
    }

    public Priority SelectedPriority
    {
        get => _selectedPriority;
        set => SetProperty(ref _selectedPriority, value);
    }

    public DateTime? NewTaskDueDate
    {
        get => _newTaskDueDate;
        set => SetProperty(ref _newTaskDueDate, value);
    }

    public string SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (SetProperty(ref _selectedFilter, value))
            {
                OnPropertyChanged(nameof(TodayVisibility));
                OnPropertyChanged(nameof(UpcomingVisibility));
                OnPropertyChanged(nameof(CompletedVisibility));
            }
        }
    }

    public Visibility TodayVisibility => SelectedFilter is "All" or "Today" ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UpcomingVisibility => SelectedFilter is "All" or "Upcoming" ? Visibility.Visible : Visibility.Collapsed;
    public Visibility CompletedVisibility => SelectedFilter is "All" or "Completed" ? Visibility.Visible : Visibility.Collapsed;

    public string TodoSearch
    {
        get => _todoSearch;
        set
        {
            if (SetProperty(ref _todoSearch, value))
            {
                RefreshGroups();
            }
        }
    }

    private async Task AddTaskAsync()
    {
        try
        {
            await _dataService.AddTodoAsync(new TodoItem
            {
                Title = NewTaskTitle.Trim(),
                Priority = SelectedPriority,
                DueDate = NewTaskDueDate ?? DateTime.Today
            });

            NewTaskTitle = string.Empty;
            NewTaskDueDate = DateTime.Today;
            RefreshGroups();
        }
        catch (Exception ex)
        {
            _notifications.ShowError(UnwrapMessage(ex));
        }
    }

    private static string UnwrapMessage(Exception ex) => ex is ServiceException se ? se.UserFacingMessage : "Something went wrong. Please try again.";

    private async void ToggleTask(object? parameter)
    {
        try
        {
            if (parameter is TodoItem todo)
            {
                await _dataService.ToggleTodoAsync(todo);
                RefreshGroups();
            }
        }
        catch (Exception ex)
        {
            _notifications.ShowError(UnwrapMessage(ex));
        }
    }

    private async void DeleteTask(object? parameter)
    {
        try
        {
            if (parameter is not TodoItem todo)
            {
                return;
            }

            await _dataService.DeleteTodoAsync(todo);
            RefreshGroups();
        }
        catch (Exception ex)
        {
            _notifications.ShowError(UnwrapMessage(ex));
        }
    }

    private void OnTodosChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshGroups();

    private void RefreshGroups()
    {
        var query = Todos.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(TodoSearch))
        {
            query = query.Where(item => item.Title.Contains(TodoSearch, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = query.ToList();
        Replace(Today, filtered.Where(item => item.DueDate?.Date <= DateTime.Today && !item.IsCompleted).OrderByDescending(item => item.Priority).ThenBy(item => item.DueDate));
        Replace(Upcoming, filtered.Where(item => item.DueDate?.Date > DateTime.Today && !item.IsCompleted).OrderBy(item => item.DueDate));
        Replace(Completed, filtered.Where(item => item.IsCompleted).OrderByDescending(item => item.CompletedAt));
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
