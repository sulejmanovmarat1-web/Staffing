using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staffing.Domain.Entities;

namespace Staffing.Infrastructure.Persistence.Configurations;

public sealed class ExcelMappingDefinitionConfiguration : IEntityTypeConfiguration<ExcelMappingDefinition>
{
    public void Configure(EntityTypeBuilder<ExcelMappingDefinition> b)
    {
        b.ToTable("excel_mapping_definitions");

        b.HasKey(x => x.Id);

        b.Property(x => x.ReportTemplateKey).HasMaxLength(128).IsRequired();
        b.Property(x => x.TemplateVersion).HasMaxLength(32).IsRequired();
        b.Property(x => x.Json).IsRequired();

        b.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Один mapping на (position + templateKey + version)
        b.HasIndex(x => new { x.PositionId, x.ReportTemplateKey, x.TemplateVersion })
            .IsUnique();

        // Для быстрого поиска активного
        b.HasIndex(x => new { x.PositionId, x.IsActive });
    }
}
