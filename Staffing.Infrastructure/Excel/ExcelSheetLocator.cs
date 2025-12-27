using ClosedXML.Excel;
using System.Text.RegularExpressions;

namespace Staffing.Infrastructure.Excel;

public static class ExcelSheetLocator
{
    public sealed record DataStart(
        string SheetName,
        int HeaderRow,
        int HeaderCol,
        int DataRowStart,
        string HeaderText
    );

    // Мы нормализуем строку до lower + без пробелов.
    // Regex допускает: "Филиалы/СП филиалов", "Филиал/СП филиала", варианты со слешем/без слеша.
    private static readonly Regex AnchorRegex = new(
        @"^филиал(ы)?/?спфилиал(ов|а)?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static DataStart FindDataStart(string filePath)
    {
        using var wb = new XLWorkbook(filePath);

        foreach (var ws in wb.Worksheets)
        {
            // Фактический размер листа
            var lastRowUsed = ws.LastRowUsed()?.RowNumber() ?? 1;
            var lastColUsed = ws.LastColumnUsed()?.ColumnNumber() ?? 1;

            // Разумные лимиты сканирования (чтобы не убивать производительность на "мусорных" листах)
            var maxRow = Math.Min(lastRowUsed, 500);
            var maxCol = Math.Min(lastColUsed, 200);

            for (var r = 1; r <= maxRow; r++)
            {
                for (var c = 1; c <= maxCol; c++)
                {
                    var cell = GetTopLeftCellConsideringMerge(ws.Cell(r, c));

                    var raw = cell.GetString();
                    if (string.IsNullOrWhiteSpace(raw)) continue;

                    var norm = Normalize(raw);
                    if (!IsAnchor(norm)) continue;

                    var dataRowStart = FindDataRowStart(ws, r, c, maxRow);
                    return new DataStart(ws.Name, r, c, dataRowStart, raw.Trim());
                }
            }
        }

        throw new InvalidOperationException(
            "Не найден старт таблицы: заголовок 'Филиалы/СП филиалов' (или близкий) не обнаружен (сканирование до 500 строк и 200 колонок).");
    }

    private static int FindDataRowStart(IXLWorksheet ws, int headerRow, int headerCol, int maxRow)
    {
        // Ищем первую строку данных ниже headerRow в той же колонке.
        // Пропускаем пустые строки и повтор/варианты якоря.
        var limit = Math.Min(headerRow + 120, maxRow);

        for (var r = headerRow + 1; r <= limit; r++)
        {
            var raw = ws.Cell(r, headerCol).GetString();
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var norm = Normalize(raw);
            if (IsAnchor(norm)) continue; // на случай повторной шапки/служебной строки

            return r;
        }

        return headerRow + 1;
    }

    private static bool IsAnchor(string normNoSpacesLower)
    {
        // Совместимость со старой логикой
        if (normNoSpacesLower == "филиалы/спфилиалов") return true;
        if (normNoSpacesLower == "филиалыспфилиалов") return true;
        if (normNoSpacesLower.StartsWith("филиалы/сп")) return true;

        return AnchorRegex.IsMatch(normNoSpacesLower);
    }

    private static IXLCell GetTopLeftCellConsideringMerge(IXLCell cell)
    {
        if (!cell.IsMerged()) return cell;
        return cell.MergedRange().FirstCell();
    }

    private static string Normalize(string s)
    {
        s = s.Trim().ToLowerInvariant();

        // NBSP -> пробел
        s = s.Replace('\u00A0', ' ');

        // Иногда встречаются "полноширинные" символы
        s = s.Replace('／', '/');

        // Переводы строк/таб -> пробел
        s = s.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");

        // Схлопываем пробелы
        s = Regex.Replace(s, @"\s+", " ").Trim();

        // Убираем пробелы вокруг слеша: " / " -> "/"
        s = Regex.Replace(s, @"\s*/\s*", "/");

        // Убираем все пробелы (как у тебя было) — остаётся компактная форма для сравнения
        s = s.Replace(" ", "");

        // Нормализуем тире
        s = s.Replace("–", "-").Replace("—", "-");

        return s;
    }
}
