using Microsoft.AspNetCore.Http;

namespace Staffing.Api.Contracts;

public sealed class ImportUploadRequest
{
    public IFormFile File { get; set; } = null!;
    public long PositionId { get; set; }
    public string ReportDate { get; set; } = null!; // YYYY-MM-DD
    public string? TemplateVersion { get; set; }
}
