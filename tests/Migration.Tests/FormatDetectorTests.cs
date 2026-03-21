using System.Text;
using RegionHR.Migration.Domain;
using RegionHR.Migration.Services;
using Xunit;

namespace RegionHR.Migration.Tests;

public class FormatDetectorTests
{
    [Fact]
    public void DetectFormat_PAXml_XmlHeader()
    {
        var content = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<paxml version=\"2.0\"><personal></personal></paxml>";
        using var stream = ToStream(content);

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.PAXml, result);
    }

    [Fact]
    public void DetectFormat_PAXml_PersonalElement()
    {
        var content = "<paxml><personal><persnr>198501151234</persnr></personal></paxml>";
        using var stream = ToStream(content);

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.PAXml, result);
    }

    [Fact]
    public void DetectFormat_PAXml_FromSampleFile()
    {
        using var stream = File.OpenRead(Path.Combine("TestData", "sample.paxml"));

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.PAXml, result);
        // Verify stream was reset
        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public void DetectFormat_SIE4i_Flagga()
    {
        var content = "#FLAGGA 0\n#SIETYP 4\n#PROGRAM \"Test\" 1.0\n";
        using var stream = ToStream(content);

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.SIE4i, result);
    }

    [Fact]
    public void DetectFormat_SIE4i_FromSampleFile()
    {
        using var stream = File.OpenRead(Path.Combine("TestData", "sample.sie"));

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.SIE4i, result);
    }

    [Fact]
    public void DetectFormat_HEROMA_SemikolonMedPersnr()
    {
        var content = "PERSNR;FNAMN;ENAMN;ANST_FORM;KOL_AVTAL;MANLON;ENHET_KOD\n198501151234;Anna;Svensson;Tillsvidare;AB;35000;VE001\n";
        using var stream = ToStream(content);

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.HEROMA, result);
    }

    [Fact]
    public void DetectFormat_HEROMA_FromSampleFile()
    {
        using var stream = File.OpenRead(Path.Combine("TestData", "sample-heroma.csv"));

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.HEROMA, result);
    }

    [Fact]
    public void DetectFormat_PersonecP_PnrFornamn()
    {
        var content = "PNR;FORNAMN;EFTERNAMN;AVDELNING\n198501151234;Anna;Svensson;Vard\n";
        using var stream = ToStream(content);

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.PersonecP, result);
    }

    [Fact]
    public void DetectFormat_Hogia_PersnrAnsttyp()
    {
        var content = "PERSNR,ANSTTYP,NAMN,LON\n198501151234,Tillsvidare,Anna Svensson,35000\n";
        using var stream = ToStream(content);

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.Hogia, result);
    }

    [Fact]
    public void DetectFormat_Fortnox()
    {
        var content = "Anställningsnummer;Personnummer;Namn;Lön\n1001;198501151234;Anna Svensson;35000\n";
        using var stream = ToStream(content);

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.Fortnox, result);
    }

    [Fact]
    public void DetectFormat_Workday_CommaSeparated()
    {
        var content = "Employee_ID,Name,Department,Salary\nE001,Anna Svensson,Vard,35000\n";
        using var stream = ToStream(content);

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.Workday, result);
    }

    [Fact]
    public void DetectFormat_SAP_PersonnelNumber()
    {
        var content = "Personnel Number;Name;Cost Center;Salary\n1001;Anna Svensson;CC01;35000\n";
        using var stream = ToStream(content);

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.SAP, result);
    }

    [Fact]
    public void DetectFormat_GenericCSV_Fallback()
    {
        var content = "id,name,value\n1,Test,123\n";
        using var stream = ToStream(content);

        var result = FormatDetector.DetectFormat(stream);

        Assert.Equal(SourceSystem.GenericCSV, result);
    }

    [Fact]
    public void DetectFormat_ResetsStreamPosition()
    {
        var content = "PERSNR;FNAMN;ENAMN\n198501151234;Anna;Svensson\n";
        using var stream = ToStream(content);

        FormatDetector.DetectFormat(stream);

        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public void GetFormatDescription_ReturnsDescription()
    {
        var desc = FormatDetector.GetFormatDescription(SourceSystem.PAXml);
        Assert.Contains("PAXml", desc);

        var heromaDesc = FormatDetector.GetFormatDescription(SourceSystem.HEROMA);
        Assert.Contains("HEROMA", heromaDesc);
    }

    private static MemoryStream ToStream(string content) =>
        new(Encoding.UTF8.GetBytes(content));
}
