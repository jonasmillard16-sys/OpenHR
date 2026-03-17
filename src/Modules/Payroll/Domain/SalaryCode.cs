using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Domain;

/// <summary>
/// Löneart med mappning till AGI-fältkod och egenskaper.
/// </summary>
public sealed class SalaryCode
{
    public string Kod { get; set; } = string.Empty;        // T.ex. "1100" = Månadslön
    public string Benamning { get; set; } = string.Empty;
    public TaxCategory Skattekategori { get; set; }
    public bool ArSemestergrundande { get; set; }
    public bool ArPensionsgrundande { get; set; }
    public bool ArOBGrundande { get; set; }
    public string? AGIFaltkod { get; set; }                  // Skatteverkets fältkod
    public bool ArAvdrag { get; set; }                       // True = avdrag
    public bool ArAktiv { get; set; } = true;

    // Vanliga lönearter
    public static SalaryCode Manadslon => new()
    {
        Kod = "1100", Benamning = "Månadslön", Skattekategori = TaxCategory.Skattepliktig,
        ArSemestergrundande = true, ArPensionsgrundande = true, AGIFaltkod = "011"
    };

    public static SalaryCode OBTillagg => new()
    {
        Kod = "1310", Benamning = "OB-tillägg", Skattekategori = TaxCategory.Skattepliktig,
        ArSemestergrundande = true, ArPensionsgrundande = true, AGIFaltkod = "011"
    };

    public static SalaryCode Overtid => new()
    {
        Kod = "1410", Benamning = "Övertidsersättning", Skattekategori = TaxCategory.Skattepliktig,
        ArSemestergrundande = false, ArPensionsgrundande = true, AGIFaltkod = "011"
    };

    public static SalaryCode Semesterlon => new()
    {
        Kod = "2700", Benamning = "Semesterlön", Skattekategori = TaxCategory.Skattepliktig,
        ArSemestergrundande = false, ArPensionsgrundande = true, AGIFaltkod = "011"
    };

    public static SalaryCode Sjuklon => new()
    {
        Kod = "3010", Benamning = "Sjuklön dag 2-14", Skattekategori = TaxCategory.Skattepliktig,
        ArSemestergrundande = true, ArPensionsgrundande = true, AGIFaltkod = "011"
    };

    public static SalaryCode Karensavdrag => new()
    {
        Kod = "3001", Benamning = "Karensavdrag", Skattekategori = TaxCategory.Skattepliktig,
        ArSemestergrundande = false, ArPensionsgrundande = false, AGIFaltkod = "011", ArAvdrag = true
    };

    public static SalaryCode Traktamente => new()
    {
        Kod = "5100", Benamning = "Inrikes traktamente", Skattekategori = TaxCategory.Traktamente,
        ArSemestergrundande = false, ArPensionsgrundande = false, AGIFaltkod = "050"
    };

    public static SalaryCode Milersattning => new()
    {
        Kod = "5200", Benamning = "Milersättning", Skattekategori = TaxCategory.Milersattning,
        ArSemestergrundande = false, ArPensionsgrundande = false, AGIFaltkod = "051"
    };
}
