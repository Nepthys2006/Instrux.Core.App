using Instrux.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Instrux.Infrastructure.Data.Configurations;

public sealed class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> builder)
    {
        builder.ToTable("ContentItems");
        builder.HasKey(content => content.Id);
        builder.Property(content => content.Title).HasMaxLength(180).IsRequired();
        builder.Property(content => content.Description).HasMaxLength(600);
        builder.Property(content => content.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(content => content.FilePath).HasMaxLength(500).IsRequired();
        builder.HasIndex(content => content.ClassId);
    }
}
