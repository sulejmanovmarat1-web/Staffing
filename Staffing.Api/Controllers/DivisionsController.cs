using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staffing.Infrastructure.Persistence;

namespace Staffing.Api.Controllers;

[ApiController]
[Route("api/divisions")]
public class DivisionsController : ControllerBase
{
    private readonly StaffingDbContext _db;

    public DivisionsController(StaffingDbContext db) => _db = db;

    // Получить корневые подразделения (ParentId == null) или детей конкретного узла
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] long? parentId, CancellationToken ct)
    {
        var q = _db.Divisions.AsNoTracking().AsQueryable();

        q = parentId is null
            ? q.Where(x => x.ParentId == null)
            : q.Where(x => x.ParentId == parentId);

        var items = await q
            .OrderBy(x => x.NameOriginal)
            .Select(x => new
            {
                x.Id,
                name = x.NameOriginal,
                x.Code,
                x.ParentId,
                x.IsTotalLevel
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    // Быстрый поиск по названию/коду (пригодится на дашборде и при отладке импорта)
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        q = (q ?? "").Trim();
        if (q.Length < 2) return Ok(Array.Empty<object>());

        var items = await _db.Divisions.AsNoTracking()
            .Where(x =>
                EF.Functions.ILike(x.NameOriginal, $"%{q}%") ||
                (x.Code != null && EF.Functions.ILike(x.Code, $"%{q}%")))
            .OrderBy(x => x.NameOriginal)
            .Take(50)
            .Select(x => new { x.Id, name = x.NameOriginal, x.Code, x.ParentId, x.IsTotalLevel })
            .ToListAsync(ct);

        return Ok(items);
    }
}
