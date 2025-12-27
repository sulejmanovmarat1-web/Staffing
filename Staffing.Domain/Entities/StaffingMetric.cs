using Staffing.Domain.Enums;

namespace Staffing.Domain.Entities;

public class StaffingMetric
{
    public long Id { get; set; }

    public long ImportId { get; set; }
    public ReportImport Import { get; set; } = null!;

    public long DivisionId { get; set; }
    public Division Division { get; set; } = null!;

    public long PositionId { get; set; }
    public Position Position { get; set; } = null!;

    public string MetricKey { get; set; } = null!;
    public decimal MetricValue { get; set; }

    public MetricUnit MetricUnit { get; set; } = MetricUnit.Count;
    public DateOnly MetricDate { get; set; }

    public MetricValueSource ValueSource { get; set; } = MetricValueSource.Provided;

    public string? Notes { get; set; }
}
