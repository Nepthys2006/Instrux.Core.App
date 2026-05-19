using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Instrux.Application.Helpers;
using Instrux.Application.Services;
using Instrux.Domain.Models;
using Instrux.Services.Interfaces;

namespace Instrux.Application.ViewModels;

public sealed class MainDashboardViewModel : ViewModelBase
{
    private readonly DataService _dataService;
    private readonly SessionService _sessionService;
    private object _currentPage;
    private string _pageTitle;

    public NotificationService NotificationService { get; }

    public MainDashboardViewModel(DataService dataService, SessionService sessionService, ITeacherService teacherService, NotificationService notificationService)
    {
        _dataService = dataService;
        _sessionService = sessionService;
        NotificationService = notificationService;

        Dashboard = new DashboardViewModel(_dataService, _sessionService);
        Classes = new ClassesViewModel(_dataService, notificationService);
        Calendar = new CalendarViewModel(_dataService, notificationService);
        Todo = new TodoViewModel(_dataService, notificationService);
        Settings = new SettingsViewModel(_sessionService, teacherService, notificationService);
        Settings.SignOutRequested += (_, _) => SignOut();

        NavigationItems =
        [
            new NavigationItemViewModel { Title = "Dashboard", IconPath = "/Resources/layout-dashboard.svg", Page = Dashboard, IsSelected = true },
            new NavigationItemViewModel { Title = "Classes", IconPath = "/Resources/presentation.svg", Page = Classes },
            new NavigationItemViewModel { Title = "Calendar", IconPath = "/Resources/calendar-fold.svg", Page = Calendar },
            new NavigationItemViewModel { Title = "To-Do", IconPath = "/Resources/square-check-big.svg", Page = Todo },
            new NavigationItemViewModel { Title = "Settings", IconPath = "/Resources/settings.svg", Page = Settings }
        ];

        Dashboard.OpenClassRequested += (_, classItem) =>
        {
            Classes.SelectedClass = Classes.Classes.FirstOrDefault(item => item.Id == classItem.Id) ?? classItem;
            Navigate(NavigationItems.First(item => item.Page == Classes));
        };

        _currentPage = Dashboard;
        _pageTitle = "Dashboard";
        NavigateCommand = new RelayCommand(Navigate);
        SignOutCommand = new RelayCommand(SignOut);

        RecentClasses = new ObservableCollection<Class>(_dataService.Classes);
        _dataService.Classes.CollectionChanged += OnClassesChanged;
    }

    public event EventHandler? SignOutRequested;

    public DashboardViewModel Dashboard { get; }
    public ClassesViewModel Classes { get; }
    public CalendarViewModel Calendar { get; }
    public TodoViewModel Todo { get; }
    public SettingsViewModel Settings { get; }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }
    public ObservableCollection<Class> RecentClasses { get; }
    public ICommand NavigateCommand { get; }
    public ICommand SignOutCommand { get; }

    public string TeacherName => _sessionService.CurrentTeacher.FullName;
    public string TeacherEmail => _sessionService.CurrentTeacher.Email;
    public string TeacherInitial => string.IsNullOrWhiteSpace(TeacherName) ? "?" : TeacherName[..1].ToUpperInvariant();

    public object CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public string PageTitle
    {
        get => _pageTitle;
        private set => SetProperty(ref _pageTitle, value);
    }

    private void Navigate(object? parameter)
    {
        if (parameter is not NavigationItemViewModel item)
        {
            return;
        }

        foreach (var navigationItem in NavigationItems)
        {
            navigationItem.IsSelected = navigationItem == item;
        }

        CurrentPage = item.Page;
        PageTitle = item.Title;
    }

    private void SignOut()
    {
        _sessionService.SignOut();
        SignOutRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnClassesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RecentClasses.Clear();
        foreach (var classItem in _dataService.Classes)
        {
            RecentClasses.Add(classItem);
        }
    }
}
