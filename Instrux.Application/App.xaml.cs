using System.Windows;
using Instrux.Application.Services;
using Instrux.Application.ViewModels;
using Instrux.Infrastructure.Data;
using Instrux.Services.Implementations;
using Instrux.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Instrux.Application;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        _host = Host.CreateDefaultBuilder()
            .UseContentRoot(basePath)
            .ConfigureAppConfiguration(configuration =>
            {
                configuration.SetBasePath(basePath);
                configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection")
                    ?? "Server=(localdb)\\MSSQLLocalDB;Database=InstruxDbLocal;Trusted_Connection=True;TrustServerCertificate=True;";

                services.AddDbContext<InstruxDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Singleton);
                services.AddSingleton<IAuthenticationService, AuthenticationService>();
                services.AddSingleton<IClassService, ClassService>();
                services.AddSingleton<IStudentService, StudentService>();
                services.AddSingleton<IAttendanceService, AttendanceService>();
                services.AddSingleton<IGradeService, GradeService>();
                services.AddSingleton<ICalendarEventService, CalendarEventService>();
                services.AddSingleton<ITodoService, TodoService>();
                services.AddSingleton<IContentService, ContentService>();
                services.AddSingleton<ITeacherService, TeacherService>();

                services.AddSingleton<SessionService>();
                services.AddSingleton<DataService>();
                services.AddTransient<AuthenticationViewModel>();
                services.AddTransient<AuthenticationWindow>();
                services.AddTransient<MainDashboardViewModel>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        var dbContext = _host.Services.GetRequiredService<InstruxDbContext>();
        await dbContext.Database.MigrateAsync();
        await RunAuthenticationFlowAsync();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }

    private async Task RunAuthenticationFlowAsync()
    {
        var dataService = _host!.Services.GetRequiredService<DataService>();

        while (true)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            dataService.Clear();

            var authenticationWindow = _host.Services.GetRequiredService<AuthenticationWindow>();
            if (authenticationWindow.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            await dataService.InitializeAsync();

            var signedOut = false;
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.SignOutRequested += (_, _) => signedOut = true;
            MainWindow = mainWindow;
            mainWindow.ShowDialog();

            if (!signedOut)
            {
                Shutdown();
                return;
            }
        }
    }
}
