using Microsoft.EntityFrameworkCore;
using Staffing.Domain.Entities;
using Staffing.Infrastructure.Persistence;

namespace Staffing.Infrastructure.ExcelMapping;

public interface IMappingResolver
{
    Task<ExcelMappingDefinition> ResolveAsync(long positionId, long? mappingId, string? templateKey, string? version, CancellationToken ct);
}

public sealed class MappingResolver : IMappingResolver
{
    private readonly StaffingDbContext _db;

    public MappingResolver(StaffingDbContext db) => _db = db;

    public async Task<ExcelMappingDefinition> ResolveAsync(
        long positionId,
        long? mappingId,
        string? templateKey,
        string? version,
        CancellationToken ct)
    {
        if (mappingId is not null)
        {
            var m = await _db.Set<ExcelMappingDefinition>()
                .FirstOrDefaultAsync(x => x.Id == mappingId.Value, ct);

            if (m is null)
                throw new InvalidOperationException($"Mapping id={mappingId.Value} not found");

            if (m.PositionId != positionId)
                throw new InvalidOperationException($"Mapping id={mappingId.Value} относится к другой должности (PositionId={m.PositionId})");

            return m;
        }

        // если задан templateKey/version — берём конкретно его, иначе активный
        if (!string.IsNullOrWhiteSpace(templateKey) && !string.IsNullOrWhiteSpace(version))
        {
            var m = await _db.Set<ExcelMappingDefinition>()
                .FirstOrDefaultAsync(x =>
                    x.PositionId == positionId &&
                    x.ReportTemplateKey == templateKey &&
                    x.TemplateVersion == version, ct);

            if (m is null)
                throw new InvalidOperationException($"Mapping not found for PositionId={positionId} template={templateKey} version={version}");

            return m;
        }

        var active = await _db.Set<ExcelMappingDefinition>()
            .Where(x => x.PositionId == positionId && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (active is null)
            throw new InvalidOperationException($"Active mapping not found for PositionId={positionId}");

        return active;
    }
}
