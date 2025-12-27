using Microsoft.EntityFrameworkCore;
using Staffing.Domain.Entities;
using Staffing.Infrastructure.Persistence.Configurations;

namespace Staffing.Infrastructure.Persistence;

public class StaffingDbContext : DbContext
{
    public StaffingDbContext(DbContextOptions<StaffingDbContext> options) : base(options) { }

    public DbSet<UnmappedColumn> UnmappedColumns => Set<UnmappedColumn>();
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<ReportImport> ReportImports => Set<ReportImport>();
    public DbSet<StaffingMetric> StaffingMetrics => Set<StaffingMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ExcelMappingDefinitionConfiguration());

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StaffingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
