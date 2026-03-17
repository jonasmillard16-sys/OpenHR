using ClosedXML.Excel;

namespace RegionHR.Infrastructure.Export;

public class ExportService
{
    public byte[] ToCsv<T>(IEnumerable<T> data, string[] headers, Func<T, string[]> rowMapper)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, System.Text.Encoding.UTF8);

        writer.WriteLine(string.Join(";", headers));
        foreach (var item in data)
            writer.WriteLine(string.Join(";", rowMapper(item).Select(EscapeCsv)));

        writer.Flush();
        return ms.ToArray();
    }

    public byte[] ToExcel<T>(IEnumerable<T> data, string sheetName, string[] headers, Func<T, object[]> rowMapper)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(sheetName);

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        int row = 2;
        foreach (var item in data)
        {
            var values = rowMapper(item);
            for (int i = 0; i < values.Length; i++)
            {
                var cell = ws.Cell(row, i + 1);
                switch (values[i])
                {
                    case decimal d: cell.Value = (double)d; break;
                    case int n: cell.Value = n; break;
                    case double dbl: cell.Value = dbl; break;
                    case DateTime dt: cell.Value = dt; break;
                    case DateOnly dateOnly: cell.Value = dateOnly.ToString("yyyy-MM-dd"); break;
                    default: cell.Value = values[i]?.ToString() ?? ""; break;
                }
            }
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
