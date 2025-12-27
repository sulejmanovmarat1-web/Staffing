using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staffing.Domain.Entities;

namespace Staffing.Infrastructure.Persistence.Configurations;

public sealed class StaffingMetricConfiguration : IEntityTypeConfiguration<StaffingMetric>
{
    public void Configure(EntityTypeBuilder<StaffingMetric> b)
    {
        b.ToTable("staffing_metric");
        b.HasKey(x => x.Id);

        b.Property(x => x.MetricKey)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.MetricValue)
            .HasPrecision(18, 4)
            .IsRequired();

        b.Property(x => x.MetricUnit).IsRequired();
        b.Property(x => x.MetricDate).IsRequired();
        b.Property(x => x.ValueSource).IsRequired();

        b.Property(x => x.Notes)
            .HasMaxLength(1000);

        b.HasOne(x => x.Import)
            .WithMany(i => i.Metrics)
            .HasForeignKey(x => x.ImportId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Division)
            .WithMany()
            .HasForeignKey(x => x.DivisionId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Защита от дублей метрик в рамках импорта
        b.HasIndex(x => new { x.ImportId, x.DivisionId, x.PositionId, x.MetricKey, x.MetricDate })
            .IsUnique();

        b.HasIndex(x => x.MetricKey);
        b.HasIndex(x => x.MetricDate);
    }
}
