using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staffing.Domain.Entities;

namespace Staffing.Infrastructure.Persistence.Configurations;

public sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> b)
    {
        b.ToTable("position");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.ReportTemplateKey)
            .HasMaxLength(100)
            .IsRequired();

        // Уникальность ключа шаблона отчёта
        b.HasIndex(x => x.ReportTemplateKey).IsUnique();

        b.HasIndex(x => x.Name);
    }
}
