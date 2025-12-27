namespace Staffing.Domain.Entities;

public class Position
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string ReportTemplateKey { get; set; } = null!;
}
