using System;

namespace Staffing.Domain.Entities;

public sealed class UnmappedColumn
{
    public long Id { get; set; }

    public long ImportId { get; set; }
    public long PositionId { get; set; }

    public string SheetName { get; set; } = null!;
    public int Col { get; set; }

    // исходный путь заголовка (как в headerPaths)
    public string HeaderPath { get; set; } = null!;

    // нормализованный (для удобства поиска)
    public string HeaderPathNormalized { get; set; } = null!;

    // когда зафиксировали
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // опционально: почему не замаппилось (например "no_rule_match")
    public string? Reason { get; set; }
}
