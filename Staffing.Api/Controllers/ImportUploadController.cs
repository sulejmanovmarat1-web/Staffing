using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staffing.Api.Contracts;
using Staffing.Domain.Entities;
using Staffing.Domain.Enums;
using Staffing.Infrastructure.Excel;
using Staffing.Infrastructure.Persistence;

namespace Staffing.Api.Controllers;

[ApiController]
[Route("api/imports")]
public class ImportUploadController : ControllerBase
{
    private readonly StaffingDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ImportUploadController(StaffingDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Upload([FromForm] ImportUploadRequest request, CancellationToken ct)
    {
        

        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { message = "File is required" });

        if (!DateOnly.TryParse(request.ReportDate, out var parsedReportDate))
            return BadRequest(new { message = "reportDate must be in format YYYY-MM-DD" });

        var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (ext != ".xlsx")
            return BadRequest(new { message = "На текущем этапе поддерживается только .xlsx" });

        var posExists = await _db.Positions.AnyAsync(x => x.Id == request.PositionId, ct);
        if (!posExists)
            return BadRequest(new { message = $"PositionId={request.PositionId} not found" });

        // 1) Hash
        string hashHex;
        await using (var stream = request.File.OpenReadStream())
        {
            var hash = await SHA256.HashDataAsync(stream, ct);
            hashHex = Convert.ToHexString(hash).ToLowerInvariant();
        }

        // 2) Save file to disk
        var uploadsDir = Path.Combine(_env.ContentRootPath, "App_Data", "uploads");
        Directory.CreateDirectory(uploadsDir);

        var storedFileName = $"{request.PositionId}_{parsedReportDate:yyyyMMdd}_{hashHex}{ext}";
        var storedPath = Path.Combine(uploadsDir, storedFileName);

        await using (var outStream = System.IO.File.Create(storedPath))
        await using (var inStream = request.File.OpenReadStream())
        {
            await inStream.CopyToAsync(outStream, ct);
        }

        var version = string.IsNullOrWhiteSpace(request.TemplateVersion) ? "v1" : request.TemplateVersion.Trim();

        // 3) Create import record
        var entity = new ReportImport
        {
            SourceFileName = request.File.FileName,
            SourceFileHash = hashHex,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = User?.Identity?.Name ?? "anonymous",
            PositionId = request.PositionId,
            ReportDate = parsedReportDate,
            TemplateVersion = version,
            Status = ImportStatus.Partial,
            DqErrorsCount = 0,
            DqWarningsCount = 0
        };

        _db.ReportImports.Add(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            var existing = await _db.ReportImports.AsNoTracking()
                .Where(x => x.PositionId == request.PositionId
                            && x.ReportDate == parsedReportDate
                            && x.SourceFileHash == hashHex
                            && x.TemplateVersion == version)
                .OrderByDescending(x => x.UploadedAt)
                .Select(x => new { x.Id })
                .FirstOrDefaultAsync(ct);

            return Conflict(new { message = "Duplicate import", existingImportId = existing?.Id });
        }

        // 4) Try locate table start
        string? sheetName = null;
        int? headerRow = null;
        string? parseError = null;
        List<HeaderPathDto>? headerPaths = null;

        try
        {
            var found = ExcelSheetLocator.FindDataStart(storedPath);
            sheetName = found.SheetName;
            headerRow = found.HeaderRow;

            // шапка: от (HeaderRow - 8) до (DataRowStart - 1)
            var headerTop = Math.Max(1, found.HeaderRow - 8);
            var headerBottom = Math.Max(found.HeaderRow, found.DataRowStart - 1);

            headerPaths = ExcelHeaderPathExtractor
                .Extract(storedPath, found.SheetName, headerTop, headerBottom, maxCols: 120)
                .Take(60)
                .Select(x => new HeaderPathDto(x.Col, x.Path))
                .ToList();
        }
        catch (Exception ex)
        {
            entity.Status = ImportStatus.Failed;
            entity.DqErrorsCount = 1;
            parseError = ex.Message;

            await _db.SaveChangesAsync(ct);
        }

        var resp = new ImportUploadResponse(
            ImportId: entity.Id,
            FileName: entity.SourceFileName,
            FileHash: entity.SourceFileHash,
            PositionId: entity.PositionId,
            ReportDate: entity.ReportDate.ToString("yyyy-MM-dd"),
            TemplateVersion: entity.TemplateVersion,
            Status: entity.Status.ToString(),
            SheetName: sheetName,
            HeaderRow: headerRow,
            ParseError: parseError,
            HeaderPaths: headerPaths
        );

        return Ok(resp);
    }
}
