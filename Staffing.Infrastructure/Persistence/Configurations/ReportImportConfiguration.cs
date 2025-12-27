using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staffing.Domain.Entities;

namespace Staffing.Infrastructure.Persistence.Configurations;

public sealed class ReportImportConfiguration : IEntityTypeConfiguration<ReportImport>
{
    public void Configure(EntityTypeBuilder<ReportImport> b)
    {
        b.ToTable("report_import");
        b.HasKey(x => x.Id);

        b.Property(x => x.SourceFileName)
            .HasMaxLength(255)
            .IsRequired();

        // SHA-256 обычно 64 hex, но оставляем запас
        b.Property(x => x.SourceFileHash)
            .HasMaxLength(128)
            .IsRequired();

        b.Property(x => x.UploadedBy)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.UploadedAt)
            .IsRequired();

        b.Property(x => x.TemplateVersion)
            .HasMaxLength(50)
            .IsRequired();

        // DateOnly в PostgreSQL маппится в date автоматически (EF Core 8+)
        b.Property(x => x.ReportDate)
            .IsRequired();

        b.Property(x => x.Status)
            .IsRequired();

        b.Property(x => x.DqErrorsCount)
            .IsRequired();

        b.Property(x => x.DqWarningsCount)
            .IsRequired();

        b.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Идемпотентность по ТЗ:
        // (position, report_date, источник/файл, template_version)
        // В качестве "источника" используем SourceFileHash.
        b.HasIndex(x => new { x.PositionId, x.ReportDate, x.SourceFileHash, x.TemplateVersion })
            .IsUnique();

        b.HasIndex(x => x.UploadedAt);

        b.HasMany(x => x.Metrics)
            .WithOne(m => m.Import)
            .HasForeignKey(m => m.ImportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
