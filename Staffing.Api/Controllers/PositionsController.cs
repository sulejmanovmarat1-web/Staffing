using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staffing.Infrastructure.Persistence;

namespace Staffing.Api.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private readonly StaffingDbContext _db;

    public PositionsController(StaffingDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var items = await _db.Positions
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.ReportTemplateKey })
            .ToListAsync(ct);

        return Ok(items);
    }
}
