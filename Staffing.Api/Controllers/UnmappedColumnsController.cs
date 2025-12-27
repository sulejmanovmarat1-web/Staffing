using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staffing.Infrastructure.Persistence;

namespace Staffing.Api.Controllers;

[ApiController]
[Route("api/imports/{importId:long}/unmapped")]
public class UnmappedColumnsController : ControllerBase
{
    private readonly StaffingDbContext _db;
    public UnmappedColumnsController(StaffingDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get(long importId, CancellationToken ct)
    {
        var rows = await _db.UnmappedColumns.AsNoTracking()
            .Where(x => x.ImportId == importId)
            .OrderBy(x => x.Col)
            .Select(x => new
            {
                x.SheetName,
                x.Col,
                x.HeaderPath,
                x.Reason,
                x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(new { importId, total = rows.Count, rows });
    }
}
