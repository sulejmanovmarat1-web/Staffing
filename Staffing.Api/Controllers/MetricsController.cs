using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staffing.Infrastructure.Persistence;

namespace Staffing.Api.Controllers;

[ApiController]
[Route("api/metrics")]
public sealed class MetricsController : ControllerBase
{
    private readonly StaffingDbContext _db;

    public MetricsController(StaffingDbContext db) => _db = db;

    // GET /api/metrics?importId=9&take=200
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] long importId, [FromQuery] int take = 200, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 2000);

        var rows = await _db.StaffingMetrics
            .AsNoTracking()
            .Include(x => x.Division)
            .Where(x => x.ImportId == importId)
            .OrderBy(x => x.Division.NameOriginal)
            .ThenBy(x => x.MetricKey)
            .ThenBy(x => x.MetricDate)
            .Take(take)
            .Select(x => new
            {
                x.Id,
                x.ImportId,
                x.DivisionId,
                Division = x.Division.NameOriginal,
                x.PositionId,
                x.MetricKey,
                x.MetricValue,
                MetricUnit = x.MetricUnit.ToString(),
                MetricDate = x.MetricDate.ToString("yyyy-MM-dd"),
                ValueSource = x.ValueSource.ToString(),
                x.Notes
            })
            .ToListAsync(ct);

        var total = await _db.StaffingMetrics
            .AsNoTracking()
            .Where(x => x.ImportId == importId)
            .CountAsync(ct);

        return Ok(new { importId, total, returned = rows.Count, rows });
    }
}
