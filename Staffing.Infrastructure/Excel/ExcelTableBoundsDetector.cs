using ClosedXML.Excel;

namespace Staffing.Infrastructure.Excel;

public static class ExcelTableBoundsDetector
{
    public sealed record Bounds(int StartRow, int EndRow);

    /// <summary>
    /// Определяет конец таблицы, двигаясь вниз по колонке ключа (обычно колонка подразделения).
    /// Таблица заканчивается, когда встречено N подряд пустых строк либо достигнут lastRowUsed.
    /// </summary>
    public static Bounds DetectByKeyColumn(
        IXLWorksheet ws,
        int startRow,
        int keyCol,
        int maxLookAheadRows = 2000,
        int emptyStreakToStop = 1)
    {
        var lastRowUsed = ws.LastRowUsed()?.RowNumber() ?? startRow;
        var hardEnd = Math.Min(lastRowUsed, startRow + maxLookAheadRows);

        var emptyStreak = 0;
        var lastDataRow = startRow - 1;

        for (var r = startRow; r <= hardEnd; r++)
        {
            var key = ws.Cell(r, keyCol).GetString();

            if (string.IsNullOrWhiteSpace(key))
            {
                emptyStreak++;
                if (emptyStreak >= emptyStreakToStop)
                    break;

                continue;
            }

            emptyStreak = 0;
            lastDataRow = r;
        }

        // Если данных не нашли — endRow будет меньше startRow
        return new Bounds(startRow, lastDataRow);
    }
}
