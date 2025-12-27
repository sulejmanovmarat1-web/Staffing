using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staffing.Domain.Entities;
using Staffing.Domain.Enums;
using Staffing.Infrastructure.Excel;
using Staffing.Infrastructure.ExcelMapping;
using Staffing.Infrastructure.Persistence;

namespace Staffing.Api.Controllers;

[ApiController]
[Route("api/imports")]
public sealed class ImportsController : ControllerBase
{
    private readonly StaffingDbContext _db;
    private readonly IMappingResolver _mappingResolver;
    private readonly IImportProcessingService _processing;

    public ImportsController(StaffingDbContext db, IMappingResolver mappingResolver, IImportProcessingService processing)
    {
        _db = db;
        _mappingResolver = mappingResolver;
        _processing = processing;
    }

    public sealed class CreateImportForm
    {
        public long PositionId { get; set; }
        public DateOnly ReportDate { get; set; }

        public long? MappingId { get; set; }
        public string? TemplateKey { get; set; }
        public string? Version { get; set; }

        public IFormFile File { get; set; } = null!;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<object>> Create([FromForm] CreateImportForm form, CancellationToken ct)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest("File is required");

        // 1) выбираем mapping
        var mapping = await _mappingResolver.ResolveAsync(
            positionId: form.PositionId,
            mappingId: form.MappingId,
            templateKey: form.TemplateKey,
            version: form.Version,
            ct: ct);

        // 2) сохраняем файл
        var uploadsRoot = Path.Combine(AppContext.BaseDirectory, "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var safeName = Path.GetFileName(form.File.FileName);
        var storedName = $"import_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}_{safeName}";
        var storedPath = Path.Combine(uploadsRoot, storedName);

        await using (var fs = System.IO.File.Create(storedPath))
            await form.File.CopyToAsync(fs, ct);

        // 3) создаём импорт
        var import = new ReportImport
        {
            PositionId = form.PositionId,
            ReportDate = form.ReportDate,

            MappingId = mapping.Id,

            OriginalFileName = safeName,
            StoredPath = storedPath,

            Status = default,
            DqWarningsCount = 0,
            DqErrorsCount = 0
        };

        _db.ReportImports.Add(import);
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            import.Id,
            import.Status,
            import.PositionId,
            import.ReportDate,
            import.MappingId,
            import.OriginalFileName
        });
    }

    [HttpPost("{id:long}/process")]
    public async Task<ActionResult<object>> Process([FromRoute] long id, CancellationToken ct)
    {
        // пометим "Processing" (если есть такой статус) — иначе можно пропустить
        var import = await _db.ReportImports.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (import is null) return NotFound();

        var result = await _processing.ProcessAsync(id, ct);

        return Ok(new
        {
            importId = id,
            result.DivisionsProcessed,
            result.MetricsSaved,
            result.UnmappedColumnsCount,
            warningsCount = result.Warnings.Count,
            errorsCount = result.Errors.Count
        });
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<object>> Get([FromRoute] long id, CancellationToken ct)
    {
        var import = await _db.ReportImports
            .Include(x => x.Mapping)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (import is null) return NotFound();

        return Ok(new
        {
            import.Id,
            import.Status,
            import.PositionId,
            import.ReportDate,
            import.DqWarningsCount,
            import.DqErrorsCount,
            import.MappingId,
            mapping = import.Mapping is null ? null : new
            {
                import.Mapping.ReportTemplateKey,
                import.Mapping.TemplateVersion,
                import.Mapping.IsActive
            },
            import.OriginalFileName
        });
    }
}
