using Microsoft.EntityFrameworkCore;
using Staffing.Domain.Entities;
using Staffing.Infrastructure.ExcelMapping;

namespace Staffing.Infrastructure.Excel;

public interface IImportProcessingService
{
    Task<ExcelImportProcessor.Result> ProcessAsync(long importId, CancellationToken ct);
}

public sealed class ImportProcessingService : IImportProcessingService
{
    private readonly Staffing.Infrastructure.Persistence.StaffingDbContext _db;
    private readonly ExcelImportProcessor _processor;

    public ImportProcessingService(Staffing.Infrastructure.Persistence.StaffingDbContext db, ExcelImportProcessor processor)
    {
        _db = db;
        _processor = processor;
    }

    public async Task<ExcelImportProcessor.Result> ProcessAsync(long importId, CancellationToken ct)
    {
        var import = await _db.ReportImports
            .Include(x => x.Mapping)
            .FirstOrDefaultAsync(x => x.Id == importId, ct);

        if (import is null)
            throw new InvalidOperationException($"Import id={importId} not found");

        if (import.Mapping is null)
            throw new InvalidOperationException($"Import id={importId} has no Mapping");

        // JSON -> temp file
        var tempMappingPath = Path.Combine(Path.GetTempPath(), $"mapping_{import.Id}_{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(tempMappingPath, import.Mapping.Json, ct);

        try
        {
            return await _processor.ProcessAsync(
                importId: import.Id,
                xlsxPath: import.StoredPath,
                mappingPath: tempMappingPath,
                ct: ct);
        }
        finally
        {
            try { File.Delete(tempMappingPath); } catch { /* ignore */ }
        }
    }
}
