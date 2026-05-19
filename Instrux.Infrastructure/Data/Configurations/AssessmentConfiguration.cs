using Instrux.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Instrux.Infrastructure.Data.Configurations;

public sealed class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("Assessments");
        builder.HasKey(assessment => assessment.Id);
        builder.Property(assessment => assessment.Name).HasMaxLength(140).IsRequired();
        builder.Property(assessment => assessment.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(assessment => assessment.MaxScore).HasPrecision(8, 2);
        builder.Property(assessment => assessment.Weight).HasPrecision(5, 2);
        builder.HasIndex(assessment => assessment.ClassId);
    }
}
