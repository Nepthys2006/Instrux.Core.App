using Instrux.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Instrux.Infrastructure.Data.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(student => student.Id);
        builder.Property(student => student.FullName).HasMaxLength(160).IsRequired();
        builder.Property(student => student.StudentId).HasMaxLength(80).IsRequired();
        builder.Property(student => student.Email).HasMaxLength(180);
        builder.HasIndex(student => new { student.ClassId, student.StudentId }).IsUnique();
    }
}
