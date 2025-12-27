using Staffing.Infrastructure.Excel;
using Xunit;

namespace Staffing.Tests.Excel;

public class ExcelSheetLocatorTests
{
    [Fact]
    public void FindDataStart_temp2_finds_anchor_and_data_start()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..",   // -> Staffing.Tests\
            "TestData",
            "temp2.xlsx"));

        Assert.True(File.Exists(path), $"Не найден файл: {path}");

        var ds = ExcelSheetLocator.FindDataStart(path);

        Assert.Equal("ППВ", ds.SheetName);
        Assert.Equal(3, ds.HeaderRow);
        Assert.Equal(2, ds.HeaderCol);
        Assert.Equal(5, ds.DataRowStart);
    }
}
