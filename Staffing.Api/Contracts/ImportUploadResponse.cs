namespace Staffing.Api.Contracts;

public sealed record ImportUploadResponse(
    long ImportId,
    string FileName,
    string FileHash,
    long PositionId,
    string ReportDate,
    string TemplateVersion,
    string Status,
    string? SheetName,
    int? HeaderRow,
    string? ParseError,
    List<HeaderPathDto>? HeaderPaths
);
