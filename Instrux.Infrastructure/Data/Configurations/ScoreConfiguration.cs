using Instrux.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Instrux.Infrastructure.Data.Configurations;

public sealed class ScoreConfiguration : IEntityTypeConfiguration<Score>
{
    public void Configure(EntityTypeBuilder<Score> builder)
    {
        builder.ToTable("Scores");
        builder.HasKey(score => score.Id);
        builder.Property(score => score.Value).HasPrecision(8, 2);
        builder.HasIndex(score => new { score.StudentId, score.AssessmentId }).IsUnique();
    }
}
