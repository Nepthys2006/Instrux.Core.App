using Instrux.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Instrux.Infrastructure.Data.Configurations;

public sealed class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");
        builder.HasKey(classItem => classItem.Id);
        builder.Property(classItem => classItem.Name).HasMaxLength(120).IsRequired();
        builder.Property(classItem => classItem.Section).HasMaxLength(80);
        builder.Property(classItem => classItem.Subject).HasConversion<string>().HasMaxLength(80).IsRequired();
        builder.Property(classItem => classItem.SchoolYear).HasMaxLength(20);
        builder.Property(classItem => classItem.Semester).HasMaxLength(40);
        builder.Property(classItem => classItem.CoverColor).HasMaxLength(20).IsRequired();
        builder.HasIndex(classItem => classItem.TeacherId);
    }
}
