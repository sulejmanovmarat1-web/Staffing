namespace Staffing.Api.Contracts;

public sealed record ImportProcessResponse(
    long ImportId,
    string Status,
    int DivisionsProcessed,
    int MetricsSaved,
    int UnmappedColumnsCount,
    List<string> Warnings,
    List<string> Errors
);
