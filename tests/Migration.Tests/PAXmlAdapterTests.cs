using RegionHR.Migration.Adapters;
using RegionHR.Migration.Domain;
using Xunit;

namespace RegionHR.Migration.Tests;

public class PAXmlAdapterTests
{
    private readonly PAXmlAdapter _adapter = new();

    [Fact]
    public void Source_ArPAXml()
    {
        Assert.Equal(SourceSystem.PAXml, _adapter.Source);
    }

    [Fact]
    public async Task ParseAsync_ParserarTvaAnstallda()
    {
        using var stream = File.OpenRead(Path.Combine("TestData", "sample.paxml"));

        var result = await _adapter.ParseAsync(stream);

        var employees = result.Records.Where(r => r.EntityType == "Employee").ToList();
        Assert.Equal(2, employees.Count);
    }

    [Fact]
    public async Task ParseAsync_ParserarPersonalFalt()
    {
        using var stream = File.OpenRead(Path.Combine("TestData", "sample.paxml"));

        var result = await _adapter.ParseAsync(stream);

        var anna = result.Records.First(r => r.EntityType == "Employee" && r.Fields.GetValueOrDefault("Fornamn") == "Anna");
        Assert.Equal("198501151234", anna.Fields["Personnummer"]);
        Assert.Equal("Svensson", anna.Fields["Efternamn"]);
        Assert.Equal("anna.svensson@region.se", anna.Fields["Epost"]);
        Assert.Equal("35000", anna.Fields["Manadslon"]);
        Assert.Equal("Tillsvidare", anna.Fields["Anstallningsform"]);
    }

    [Fact]
    public async Task ParseAsync_ParserarLonetransaktioner()
    {
        using var stream = File.OpenRead(Path.Combine("TestData", "sample.paxml"));

        var result = await _adapter.ParseAsync(stream);

        var payroll = result.Records.Where(r => r.EntityType == "PayrollRecord").ToList();
        Assert.Equal(2, payroll.Count);
        Assert.Equal("1000", payroll[0].Fields["Loneart"]);
        Assert.Equal("35000", payroll[0].Fields["Belopp"]);
    }

    [Fact]
    public async Task ParseAsync_ParserarTidtransaktioner()
    {
        using var stream = File.OpenRead(Path.Combine("TestData", "sample.paxml"));

        var result = await _adapter.ParseAsync(stream);

        var time = result.Records.Where(r => r.EntityType == "TimeRecord").ToList();
        Assert.Single(time);
        Assert.Equal("NARV", time[0].Fields["Tidkod"]);
        Assert.Equal("8", time[0].Fields["Timmar"]);
    }

    [Fact]
    public async Task ParseAsync_SatterTotalRows()
    {
        using var stream = File.OpenRead(Path.Combine("TestData", "sample.paxml"));

        var result = await _adapter.ParseAsync(stream);

        // 2 employees + 2 payroll + 1 time = 5
        Assert.Equal(5, result.TotalRows);
    }

    [Fact]
    public async Task ParseAsync_TomtDokument_GerVarning()
    {
        using var stream = new MemoryStream("<root></root>"u8.ToArray());

        var result = await _adapter.ParseAsync(stream);

        Assert.Empty(result.Records);
        Assert.Equal(0, result.TotalRows);
    }

    [Fact]
    public void GetDefaultMappings_ReturnerarMappningar()
    {
        var mappings = _adapter.GetDefaultMappings();

        Assert.NotEmpty(mappings);
        Assert.Contains(mappings, m => m.KallFalt == "persnr" && m.MalFalt == "Personnummer");
        Assert.Contains(mappings, m => m.KallFalt == "fornamn" && m.MalFalt == "Fornamn");
    }
}
