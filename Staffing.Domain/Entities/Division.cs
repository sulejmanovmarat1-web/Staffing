namespace Staffing.Domain.Entities;

public class Division
{
    public long Id { get; set; }
    public string NameOriginal { get; set; } = null!;
    public string? Code { get; set; }

    public long? ParentId { get; set; }
    public Division? Parent { get; set; }
    public List<Division> Children { get; set; } = new();

    public bool IsTotalLevel { get; set; }
}
