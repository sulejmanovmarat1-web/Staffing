namespace Staffing.Infrastructure.ExcelMapping;

public sealed class ExcelMappingConfig
{
    public string report_template_key { get; set; } = null!;
    public string template_version { get; set; } = "v1";
    public List<ExcelMappingRule> mappings { get; set; } = new();
}

public sealed class ExcelMappingRule
{
    public string header_path { get; set; } = null!;   // regex
    public string metric_key { get; set; } = null!;
    public string unit { get; set; } = "count";        // count|percent
    public string date_binding { get; set; } = "report_date"; // report_date|date_from_header

    public int? col { get; set; } // если задано — правило только для этой колонки

}
