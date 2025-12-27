using System.Text.Json;

namespace Staffing.Infrastructure.ExcelMapping;

public static class MappingLoader
{

    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ExcelMappingConfig Load(string filePath)
    {
        var json = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException($"Mapping file is empty: {filePath}");

        try
        {
            var cfg = JsonSerializer.Deserialize<ExcelMappingConfig>(json, _opts);
            if (cfg is null)
                throw new InvalidOperationException($"Mapping file has invalid JSON: {filePath}");

            cfg.mappings ??= new List<ExcelMappingRule>();
            return cfg;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Mapping file is invalid JSON: {filePath}", ex);
        }
    }

}
