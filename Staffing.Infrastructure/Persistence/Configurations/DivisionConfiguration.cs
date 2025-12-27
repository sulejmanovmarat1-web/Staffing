using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staffing.Domain.Entities;

namespace Staffing.Infrastructure.Persistence.Configurations;

public sealed class DivisionConfiguration : IEntityTypeConfiguration<Division>
{
    public void Configure(EntityTypeBuilder<Division> b)
    {
        b.ToTable("division");
        b.HasKey(x => x.Id);

        b.Property(x => x.NameOriginal)
            .HasMaxLength(300)
            .IsRequired();

        b.Property(x => x.Code)
            .HasMaxLength(50);

        b.Property(x => x.IsTotalLevel)
            .IsRequired();

        b.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.NameOriginal);
        b.HasIndex(x => x.Code);
        b.HasIndex(x => x.ParentId);
    }
}
