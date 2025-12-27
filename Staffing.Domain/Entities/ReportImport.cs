using Staffing.Domain.Enums;

namespace Staffing.Domain.Entities;

public class ReportImport
{
    public long Id { get; set; }

    public string SourceFileName { get; set; } = null!;
    public string SourceFileHash { get; set; } = null!;

    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = null!;

    public long PositionId { get; set; }
    public Position Position { get; set; } = null!;

    public DateOnly ReportDate { get; set; }

    public string TemplateVersion { get; set; } = "v1";

    public ImportStatus Status { get; set; } = ImportStatus.Ok;
    public int DqErrorsCount { get; set; }
    public int DqWarningsCount { get; set; }
    public long? MappingId { get; set; }
    public ExcelMappingDefinition? Mapping { get; set; }

    public string OriginalFileName { get; set; } = null!;
    public string StoredPath { get; set; } = null!;
    public List<StaffingMetric> Metrics { get; set; } = new();
}
