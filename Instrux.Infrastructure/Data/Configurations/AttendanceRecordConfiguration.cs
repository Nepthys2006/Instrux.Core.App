using Instrux.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Instrux.Infrastructure.Data.Configurations;

public sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(record => record.Note).HasMaxLength(240);
        builder.HasIndex(record => new { record.StudentId, record.Date }).IsUnique();
    }
}
