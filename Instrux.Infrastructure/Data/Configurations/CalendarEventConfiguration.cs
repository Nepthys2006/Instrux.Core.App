using Instrux.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Instrux.Infrastructure.Data.Configurations;

public sealed class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ToTable("CalendarEvents");
        builder.HasKey(calendarEvent => calendarEvent.Id);
        builder.Property(calendarEvent => calendarEvent.Title).HasMaxLength(160).IsRequired();
        builder.Property(calendarEvent => calendarEvent.Category).HasConversion<string>().HasMaxLength(60).IsRequired();
        builder.Property(calendarEvent => calendarEvent.Notes).HasMaxLength(600);
        builder.HasIndex(calendarEvent => new { calendarEvent.TeacherId, calendarEvent.Date });
    }
}
