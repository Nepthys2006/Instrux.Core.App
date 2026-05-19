using Instrux.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Instrux.Infrastructure.Data.Configurations;

public sealed class GradingConfigConfiguration : IEntityTypeConfiguration<GradingConfig>
{
    public void Configure(EntityTypeBuilder<GradingConfig> builder)
    {
        builder.ToTable("GradingConfigs");
        builder.HasKey(config => config.Id);
        builder.Property(config => config.Subject).HasConversion<string>().HasMaxLength(80).IsRequired();
        builder.Property(config => config.Group).HasConversion<string>().HasMaxLength(80).IsRequired();
        builder.Property(config => config.WrittenWorksWeight).HasPrecision(5, 2);
        builder.Property(config => config.PerformanceTasksWeight).HasPrecision(5, 2);
        builder.Property(config => config.QuarterlyAssessmentWeight).HasPrecision(5, 2);
        builder.HasIndex(config => config.Subject).IsUnique();
    }
}
