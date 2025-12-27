using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Staffing.Domain.Entities;
using Staffing.Domain.Enums;
using Staffing.Infrastructure.ExcelMapping;
using Staffing.Infrastructure.Persistence;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Staffing.Infrastructure.Excel;

public sealed class ExcelImportProcessor
{
    private static readonly Regex DateRegex = new(@"(\d{2}\.\d{2}\.\d{4})", RegexOptions.Compiled);

    private readonly StaffingDbContext _db;

    public ExcelImportProcessor(StaffingDbContext db) => _db = db;

    public sealed record Result(
        int DivisionsProcessed,
        int MetricsSaved,
        int UnmappedColumnsCount,
        List<string> Warnings,
        List<string> Errors
    );

    public async Task<Result> ProcessAsync(
        long importId,
        string xlsxPath,
        string mappingPath,
        CancellationToken ct)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        var import = await _db.ReportImports
            .Include(x => x.Position)
            .FirstOrDefaultAsync(x => x.Id == importId, ct);

        if (import is null)
            throw new InvalidOperationException($"Import id={importId} not found");

        if (!File.Exists(xlsxPath))
            throw new FileNotFoundException($"XLSX file not found: {xlsxPath}");

        if (!File.Exists(mappingPath))
            throw new FileNotFoundException($"Mapping json not found: {mappingPath}");

        var mapping = MappingLoader.Load(mappingPath);

        // 0) Разрешаем перезапуск обработки: чистим старые результаты импорта
        _db.StaffingMetrics.RemoveRange(_db.StaffingMetrics.Where(x => x.ImportId == importId));
        _db.UnmappedColumns.RemoveRange(_db.UnmappedColumns.Where(x => x.ImportId == importId));
        await _db.SaveChangesAsync(ct);

        // 1) Найдём старт таблицы
        var start = ExcelSheetLocator.FindDataStart(xlsxPath);
        var sheetName = start.SheetName;

        // 2) Вытащим header paths по всей шапке
        var headerTop = Math.Max(1, start.HeaderRow - 8);
        var headerBottom = Math.Max(start.HeaderRow, start.DataRowStart - 1);

        var headers = ExcelHeaderPathExtractor
            .Extract(xlsxPath, sheetName, headerTop, headerBottom, maxCols: 200);

        // быстрый доступ: col -> headerPath
        var headerPathByCol = headers
            .GroupBy(h => h.Col)
            .ToDictionary(g => g.Key, g => g.First().Path);

        // Глобальный индекс на весь импорт: ImportId|DivisionId|PositionId|MetricKey|MetricDate -> metric
        // (чтобы быстро ловить дубли и не падать на уникальном индексе БД)
        var metricIndex = new Dictionary<string, StaffingMetric>(StringComparer.OrdinalIgnoreCase);

        // Буфер для журнала несопоставленных колонок (запишем в БД одним батчем)
        var unmappedToSave = new List<UnmappedColumn>();

        // 3) Сопоставим колонки по regex rules
        var colRules = new Dictionary<int, ExcelMappingRule>();
        var unmappedCols = 0;

        foreach (var h in headers)
        {
            var hp = NormalizeHeader(h.Path);

            ExcelMappingRule? matched = null;

            // 1) сначала точные правила по колонке
            foreach (var rule in mapping.mappings)
            {
                if (rule.col is null) continue;
                if (rule.col.Value != h.Col) continue;

                if (Regex.IsMatch(hp, rule.header_path, RegexOptions.IgnoreCase))
                {
                    matched = rule;
                    break;
                }
            }

            // 2) если не нашли — ищем среди общих
            if (matched is null)
            {
                foreach (var rule in mapping.mappings)
                {
                    if (rule.col is not null) continue;

                    if (Regex.IsMatch(hp, rule.header_path, RegexOptions.IgnoreCase))
                    {
                        matched = rule;
                        break;
                    }
                }
            }

            // 3) если вообще не нашли — считаем как unmapped и идём дальше
            if (matched is null)
            {
                unmappedCols++;

                unmappedToSave.Add(new UnmappedColumn
                {
                    ImportId = import.Id,
                    PositionId = import.PositionId,
                    SheetName = sheetName,
                    Col = h.Col,
                    HeaderPath = h.Path ?? "",
                    HeaderPathNormalized = NormalizeHeader(h.Path ?? ""),
                    Reason = "no_rule_match"
                });

                continue;
            }

            // 4) игнорируем “виртуальный” mapping на division name
            if (matched.metric_key == "__division_name__")
                continue;

            colRules[h.Col] = matched;
        }

        // Если вообще ни одной колонки не сопоставили — это критично
        if (colRules.Count == 0)
        {
            errors.Add($"No mapped columns for template '{mapping.report_template_key}'. Check mapping rules/header_path.");

            import.DqWarningsCount = warnings.Count;
            import.DqErrorsCount = errors.Count;
            import.Status = ImportStatus.Failed;

            await _db.SaveChangesAsync(ct);

            // даже если хотим сохранить unmapped — можно, но смысла мало
            return new Result(
                DivisionsProcessed: 0,
                MetricsSaved: 0,
                UnmappedColumnsCount: unmappedCols,
                Warnings: warnings,
                Errors: errors);
        }

        // 3.1) Сохраним журнал несопоставленных колонок (до обработки строк)
        if (unmappedToSave.Count > 0)
        {
            // убираем дубли по (ImportId, SheetName, Col)
            unmappedToSave = unmappedToSave
                .GroupBy(x => new { x.ImportId, x.SheetName, x.Col })
                .Select(g => g.First())
                .ToList();

            // актуализируем количество (на случай дублей)
            unmappedCols = unmappedToSave.Count;

            _db.UnmappedColumns.AddRange(unmappedToSave);
            await _db.SaveChangesAsync(ct);
        }

        // 4) Подготовим чтение файла
        using var wb = new XLWorkbook(xlsxPath);
        var ws = wb.Worksheet(sheetName);

        // 5) Читаем строки подразделений
        var divisionsProcessed = 0;
        var metricsSaved = 0;

        var divCol = start.HeaderCol; // где стоит "Филиалы/СП филиалов"

        var bounds = ExcelTableBoundsDetector.DetectByKeyColumn(
            ws,
            startRow: start.DataRowStart,
            keyCol: start.HeaderCol,
            maxLookAheadRows: 2000,
            emptyStreakToStop: 2); // 2 — чтобы одиночная пустая строка не обрывала таблицу

        if (bounds.EndRow < bounds.StartRow)
            throw new InvalidOperationException("Таблица найдена, но строки данных отсутствуют.");

        for (var row = bounds.StartRow; row <= bounds.EndRow; row++)
        {
            var divName = ws.Cell(row, divCol).GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(divName))
                continue;

            // Итоговая строка (очень грубо): “ГОРЬК”
            var isTotal = string.Equals(divName, "ГОРЬК", StringComparison.OrdinalIgnoreCase);

            // upsert Division по NameOriginal
            var division = await _db.Divisions.FirstOrDefaultAsync(x => x.NameOriginal == divName, ct);
            if (division is null)
            {
                division = new Division
                {
                    NameOriginal = divName,
                    Code = null,
                    ParentId = null,
                    IsTotalLevel = isTotal
                };
                _db.Divisions.Add(division);
                await _db.SaveChangesAsync(ct);
            }
            else if (isTotal && !division.IsTotalLevel)
            {
                division.IsTotalLevel = true;
                await _db.SaveChangesAsync(ct);
            }

            divisionsProcessed++;

            // защита от дублей в рамках строки Excel (именно по DB-уникальности)
            var seenInRow = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in colRules)
            {
                var col = kv.Key;
                var rule = kv.Value;

                var cell = ws.Cell(row, col);

                // если в ячейке Excel-ошибка (#DIV/0!, #N/A, #VALUE!, …) — пропускаем
                if (cell.DataType == XLDataType.Error)
                    continue;

                var raw = cell.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                // также отловим ошибки, которые пришли как текст
                if (raw.StartsWith("#", StringComparison.Ordinal))
                    continue;

                if (!TryParseDecimal(raw, out var value))
                {
                    warnings.Add($"Row {row}, Col {col}: cannot parse number '{raw}'");
                    continue;
                }

                var isPercent = rule.unit.Equals("percent", StringComparison.OrdinalIgnoreCase);

                // unit
                var unit = isPercent ? MetricUnit.Percent : MetricUnit.Count;

                // date binding
                var metricDate = import.ReportDate;
                if (rule.date_binding.Equals("date_from_header", StringComparison.OrdinalIgnoreCase))
                {
                    var headerPath = headerPathByCol.TryGetValue(col, out var hp) ? hp : "";
                    var m = DateRegex.Match(headerPath);
                    if (m.Success && DateOnly.TryParseExact(
                            m.Groups[1].Value,
                            "dd.MM.yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var d))
                    {
                        metricDate = d;
                    }
                }

                // Нормализация процентов:
                // делим на 100 ТОЛЬКО если |value| > 2 (т.е. похоже на "57", а не на "0.57")
                if (isPercent && decimal.Abs(value) > 2m)
                    value /= 100m;

                // ключ, который совпадает с уникальным индексом БД
                var uniqDb = $"{import.Id}|{division.Id}|{import.PositionId}|{rule.metric_key}|{metricDate:yyyy-MM-dd}";

                if (!seenInRow.Add(uniqDb))
                {
                    warnings.Add($"Duplicate metric in row {row}: {rule.metric_key} date={metricDate:yyyy-MM-dd} (col {col}) - skipped");
                    continue;
                }

                if (metricIndex.TryGetValue(uniqDb, out var existingMetric))
                {
                    // Перезаписываем последним значением (чтобы не падать)
                    existingMetric.MetricValue = value;
                    existingMetric.MetricUnit = unit;
                    existingMetric.ValueSource = MetricValueSource.Provided;
                    existingMetric.Notes = $"Duplicate in Excel (row {row}, col {col}) - overwritten";

                    warnings.Add($"Duplicate metric for division '{division.NameOriginal}': {rule.metric_key} {metricDate:yyyy-MM-dd} (row {row}, col {col}) overwritten");
                    continue;
                }

                var metric = new StaffingMetric
                {
                    ImportId = import.Id,
                    DivisionId = division.Id,
                    PositionId = import.PositionId,
                    MetricKey = rule.metric_key,
                    MetricValue = value,
                    MetricUnit = unit,
                    MetricDate = metricDate,
                    ValueSource = MetricValueSource.Provided,
                    Notes = null
                };

                metricIndex[uniqDb] = metric;
                _db.StaffingMetrics.Add(metric);
                metricsSaved++;
            }

            if (divisionsProcessed % 200 == 0)
                await _db.SaveChangesAsync(ct);
        }

        await _db.SaveChangesAsync(ct);

        if (divisionsProcessed > 0 && metricsSaved == 0)
            errors.Add("No metrics were saved although divisions were detected. Mapping may not match headers or all values are empty.");

        // 6) Финальный статус
        import.DqWarningsCount = warnings.Count;
        import.DqErrorsCount = errors.Count;

        import.Status = errors.Count > 0
            ? ImportStatus.Failed
            : (warnings.Count > 0 ? ImportStatus.Partial : ImportStatus.Ok);

        await _db.SaveChangesAsync(ct);

        return new Result(divisionsProcessed, metricsSaved, unmappedCols, warnings, errors);
    }

    private static string NormalizeHeader(string s)
    {
        s = (s ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
        s = Regex.Replace(s, @"\s+", " ").Trim();
        return s;
    }

    private static bool TryParseDecimal(string raw, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var s = raw.Trim();

        // Excel иногда даёт "57%" или " 57 % "
        s = s.Replace("%", "");

        // NBSP и обычные пробелы
        s = s.Replace("\u00A0", "").Replace(" ", "");

        // минус может быть "−"
        s = s.Replace("−", "-");

        // (123) -> -123
        if (s.Length >= 2 && s.StartsWith("(") && s.EndsWith(")"))
            s = "-" + s[1..^1];

        // запятая в дробях
        s = s.Replace(",", ".");

        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }
}
