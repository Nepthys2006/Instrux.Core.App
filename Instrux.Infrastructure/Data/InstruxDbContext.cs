using Instrux.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Infrastructure.Data;

public sealed class InstruxDbContext : DbContext
{
    public InstruxDbContext(DbContextOptions<InstruxDbContext> options)
        : base(options)
    {
    }

    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<GradingConfig> GradingConfigs => Set<GradingConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InstruxDbContext).Assembly);
    }
}
