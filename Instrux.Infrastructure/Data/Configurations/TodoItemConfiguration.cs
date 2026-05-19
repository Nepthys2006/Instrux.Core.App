using Instrux.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Instrux.Infrastructure.Data.Configurations;

public sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("TodoItems");
        builder.HasKey(todo => todo.Id);
        builder.Property(todo => todo.Title).HasMaxLength(180).IsRequired();
        builder.Property(todo => todo.Priority).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(todo => todo.Recurrence).HasConversion<string>().HasMaxLength(40);
        builder.HasIndex(todo => new { todo.TeacherId, todo.DueDate });
    }
}
