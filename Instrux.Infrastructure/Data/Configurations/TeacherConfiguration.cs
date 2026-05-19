using Instrux.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Instrux.Infrastructure.Data.Configurations;

public sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");
        builder.HasKey(teacher => teacher.Id);
        builder.Property(teacher => teacher.FullName).HasMaxLength(160).IsRequired();
        builder.Property(teacher => teacher.Nickname).HasMaxLength(80).IsRequired();
        builder.Property(teacher => teacher.Email).HasMaxLength(180).IsRequired();
        builder.Property(teacher => teacher.PasswordHash).HasMaxLength(256).IsRequired();
        builder.HasIndex(teacher => teacher.Email).IsUnique();
    }
}
