using ClosedXML.Excel;

namespace Staffing.Infrastructure.Excel;

public static class ExcelHeaderPathExtractor
{
    public sealed record ColumnHeader(int Col, string Path);

    public static List<ColumnHeader> Extract(
        string filePath,
        string sheetName,
        int headerTopRow,
        int headerBottomRow,
        int maxCols = 120)
    {
        using var wb = new XLWorkbook(filePath);
        var ws = wb.Worksheet(sheetName);

        var lastColUsed = ws.LastColumnUsed()?.ColumnNumber() ?? maxCols;
        var maxCol = Math.Min(lastColUsed, maxCols);

        var result = new List<ColumnHeader>(maxCol);

        for (var c = 1; c <= maxCol; c++)
        {
            var parts = new List<string>();

            for (var r = headerTopRow; r <= headerBottomRow; r++)
            {
                var cell = ws.Cell(r, c);

                string text;
                if (cell.IsMerged())
                    text = cell.MergedRange().FirstCell().GetString();
                else
                    text = cell.GetString();

                text = (text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;

                // нормализуем переносы
                text = text.Replace("\r", " ").Replace("\n", " ").Trim();

                // 1) пропускаем титульный заголовок, который повторяется везде
                if (text.StartsWith("Сведения об укомплектованности", StringComparison.OrdinalIgnoreCase))
                    continue;

                // 2) пропускаем “пустые” служебные значения (по желанию расширим список)
                if (string.Equals(text, "Сведения", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (parts.Count == 0 || !string.Equals(parts[^1], text, StringComparison.OrdinalIgnoreCase))
                    parts.Add(text);
            }

            var path = string.Join(" / ", parts);

            // на всякий случай вырежем общий префикс, если всё-таки попал
            path = RemoveCommonPrefix(path);
            if (!string.IsNullOrWhiteSpace(path))
                result.Add(new ColumnHeader(c, path));
        }

        return result;
    }
    private static string RemoveCommonPrefix(string path)
    {
        const string prefix = "Сведения об укомплектованности";
        if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            // убираем до первого разделителя, если он есть
            var idx = path.IndexOf(" / ", StringComparison.Ordinal);
            if (idx > 0) return path[(idx + 3)..].Trim();
        }
        return path;
    }
}
