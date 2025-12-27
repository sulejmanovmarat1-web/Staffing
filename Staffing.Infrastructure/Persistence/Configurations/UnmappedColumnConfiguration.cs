using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staffing.Domain.Entities;

namespace Staffing.Infrastructure.Persistence.Configurations;

public sealed class UnmappedColumnConfiguration : IEntityTypeConfiguration<UnmappedColumn>
{
    public void Configure(EntityTypeBuilder<UnmappedColumn> b)
    {
        b.ToTable("unmapped_columns");

        b.HasKey(x => x.Id);

        b.Property(x => x.SheetName).HasMaxLength(128).IsRequired();
        b.Property(x => x.HeaderPath).HasMaxLength(1000).IsRequired();
        b.Property(x => x.HeaderPathNormalized).HasMaxLength(1000).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(256);

        // Чтобы не дублировать одно и то же при повторном process
        b.HasIndex(x => new { x.ImportId, x.SheetName, x.Col })
         .IsUnique();

        // Быстрые выборки по импорту
        b.HasIndex(x => x.ImportId);
    }
}
