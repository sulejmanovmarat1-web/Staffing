using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staffing.Api.Contracts;
using Staffing.Infrastructure.Excel;
using Staffing.Infrastructure.Persistence;

namespace Staffing.Api.Controllers;

[ApiController]
[Route("api/imports")]
public class ImportProcessController : ControllerBase
{
    private readonly StaffingDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ExcelImportProcessor _processor;

    public ImportProcessController(StaffingDbContext db, IWebHostEnvironment env, ExcelImportProcessor processor)
    {
        _db = db;
        _env = env;
        _processor = processor;
    }

    [HttpPost("{importId:long}/process")]
    public async Task<IActionResult> Process(long importId, CancellationToken ct)
    {
        var imp = await _db.ReportImports
            .Include(x => x.Position)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == importId, ct);


        if (imp is null)
            return NotFound(new { message = $"Import id={importId} not found" });

        // путь к сохранённому xlsx (как в upload)
        var uploadsDir = Path.Combine(_env.ContentRootPath, "App_Data", "uploads");
        var xlsxPath = Path.Combine(uploadsDir, $"{imp.PositionId}_{imp.ReportDate:yyyyMMdd}_{imp.SourceFileHash}.xlsx");

        // mapping: пока выбираем по позиции и версии
        // Например: provodniki_v1.json
        // (если у тебя другое имя — поменяй здесь)
        var mappingDir = Path.Combine(_env.ContentRootPath, "App_Data", "mapping");

        // report_template_key берём из справочника Position
        var key = imp.Position?.ReportTemplateKey;

        if (string.IsNullOrWhiteSpace(key))
            return BadRequest(new { message = $"PositionId={imp.PositionId} has empty ReportTemplateKey. Cannot select mapping." });

        // template version (у тебя в модели есть imp.TemplateVersion)
        var version = 1;

        if (!string.IsNullOrWhiteSpace(imp.TemplateVersion) &&
            int.TryParse(imp.TemplateVersion.Trim(), out var parsed) &&
            parsed > 0)
        {
            version = parsed;
        }

        var mappingFileName = $"{key}_v{version}.json";
        var mappingPath = Path.Combine(mappingDir, mappingFileName);

        if (!System.IO.File.Exists(mappingPath))
            return NotFound(new { message = $"Mapping file not found: {mappingFileName}" });


        var r = await _processor.ProcessAsync(importId, xlsxPath, mappingPath, ct);

        var status = (await _db.ReportImports.AsNoTracking()
            .Where(x => x.Id == importId)
            .Select(x => x.Status)
            .FirstAsync(ct)).ToString();

        return Ok(new ImportProcessResponse(
            ImportId: importId,
            Status: status,
            DivisionsProcessed: r.DivisionsProcessed,
            MetricsSaved: r.MetricsSaved,
            UnmappedColumnsCount: r.UnmappedColumnsCount,
            Warnings: r.Warnings.Take(50).ToList(),
            Errors: r.Errors.Take(50).ToList()
        ));
    }
}
