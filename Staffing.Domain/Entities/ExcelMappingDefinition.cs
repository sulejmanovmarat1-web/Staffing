using System;

namespace Staffing.Domain.Entities;

public sealed class ExcelMappingDefinition
{
    public long Id { get; set; }

    // Например: "provodniki", "osmotrshchiki_remontniki"
    public string ReportTemplateKey { get; set; } = null!;

    // Например: "v1"
    public string TemplateVersion { get; set; } = null!;

    public long PositionId { get; set; }
    public Position Position { get; set; } = null!;

    public bool IsActive { get; set; }

    // Содержимое JSON mapping-файла целиком
    public string Json { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
