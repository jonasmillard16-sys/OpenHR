using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Domain;

/// <summary>
/// Komplett uppsättning av lönearter för regionalt HR-system.
/// Mappar till AGI-fältkoder och anger egenskaper som
/// semestergrundande, pensionsgrundande, skattekategori m.m.
/// </summary>
public static class SalaryCodeSeed
{
    public static IReadOnlyList<SalaryCode> GetAll() =>
    [
        // === Grundlön ===
        new SalaryCode
        {
            Kod = "1100", Benamning = "Månadslön",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "1200", Benamning = "Timlön",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },

        // === OB-tillägg ===
        new SalaryCode
        {
            Kod = "1310", Benamning = "OB-tillägg vardagkväll",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            ArOBGrundande = false, AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "1320", Benamning = "OB-tillägg vardagnatt",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            ArOBGrundande = false, AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "1330", Benamning = "OB-tillägg helg",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            ArOBGrundande = false, AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "1340", Benamning = "OB-tillägg storhelg",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            ArOBGrundande = false, AGIFaltkod = "011", ArAvdrag = false
        },

        // === Övertid ===
        new SalaryCode
        {
            Kod = "1410", Benamning = "Enkel övertid",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "1420", Benamning = "Kvalificerad övertid",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },

        // === Jour och beredskap ===
        new SalaryCode
        {
            Kod = "1500", Benamning = "Jour",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "1510", Benamning = "Beredskap",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },

        // === Semester ===
        new SalaryCode
        {
            Kod = "2700", Benamning = "Semesterlön",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "2710", Benamning = "Semesterlöneavdrag",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "011", ArAvdrag = true
        },
        new SalaryCode
        {
            Kod = "2720", Benamning = "Semestertillägg",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "2730", Benamning = "Semesterersättning",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },

        // === Sjukfrånvaro ===
        new SalaryCode
        {
            Kod = "3001", Benamning = "Karensavdrag",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "011", ArAvdrag = true
        },
        new SalaryCode
        {
            Kod = "3010", Benamning = "Sjuklön dag 2-14",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "3020", Benamning = "Sjuklön dag 15-90 (FK-komplement)",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },

        // === Föräldraledighet ===
        new SalaryCode
        {
            Kod = "3100", Benamning = "Föräldralöneutfyllnad",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },

        // === Pension ===
        new SalaryCode
        {
            Kod = "4100", Benamning = "AKAP-KR avgift",
            Skattekategori = TaxCategory.Skattefri,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "062", ArAvdrag = false
        },

        // === Resekostnader ===
        new SalaryCode
        {
            Kod = "5100", Benamning = "Inrikes traktamente",
            Skattekategori = TaxCategory.Traktamente,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "050", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "5110", Benamning = "Inrikes traktamente skattepliktigt",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "5200", Benamning = "Milersättning",
            Skattekategori = TaxCategory.Milersattning,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "051", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "5210", Benamning = "Milersättning skattepliktigt",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "011", ArAvdrag = false
        },

        // === Avdrag ===
        new SalaryCode
        {
            Kod = "6100", Benamning = "Fackavgift",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = string.Empty, ArAvdrag = true
        },
        new SalaryCode
        {
            Kod = "6200", Benamning = "Löneutmätning",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = string.Empty, ArAvdrag = true
        },
        new SalaryCode
        {
            Kod = "6300", Benamning = "Bruttolöneavdrag",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = string.Empty, ArAvdrag = true
        },

        // === Retroaktiva lönearter ===
        new SalaryCode
        {
            Kod = "7100", Benamning = "Retro månadslön",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "7110", Benamning = "Retro OB-tillägg",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "7120", Benamning = "Retro övertid",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "7130", Benamning = "Retro jour",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "7140", Benamning = "Retro beredskap",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "7150", Benamning = "Retro skattejustering",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "011", ArAvdrag = false
        },

        // === Övriga tillägg ===
        new SalaryCode
        {
            Kod = "1600", Benamning = "Fyllnadslön",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "1610", Benamning = "Risktillägg",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "1620", Benamning = "Handledartillägg",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "1630", Benamning = "Kväll-/nattillägg fast",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "1700", Benamning = "Lönetillägg personligt",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "1710", Benamning = "Lönetillägg tidsbegränsat",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true,
            AGIFaltkod = "011", ArAvdrag = false
        },

        // === Frånvaroavdrag ===
        new SalaryCode
        {
            Kod = "3200", Benamning = "Tjänstledigavdrag",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "011", ArAvdrag = true
        },
        new SalaryCode
        {
            Kod = "3210", Benamning = "Föräldraled. avdrag",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "011", ArAvdrag = true
        },
        new SalaryCode
        {
            Kod = "3220", Benamning = "VAB-avdrag",
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "011", ArAvdrag = true
        },

        // === Förmåner ===
        new SalaryCode
        {
            Kod = "8100", Benamning = "Förmån fri bil",
            Skattekategori = TaxCategory.Forman,
            ArSemestergrundande = false, ArPensionsgrundande = true,
            AGIFaltkod = "012", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "8110", Benamning = "Förmån fritt boende",
            Skattekategori = TaxCategory.Forman,
            ArSemestergrundande = false, ArPensionsgrundande = true,
            AGIFaltkod = "012", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "8200", Benamning = "Förmån fri sjukvård",
            Skattekategori = TaxCategory.Forman,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = "012", ArAvdrag = false
        },
        new SalaryCode
        {
            Kod = "8300", Benamning = "Friskvårdsbidrag",
            Skattekategori = TaxCategory.Skattefri,
            ArSemestergrundande = false, ArPensionsgrundande = false,
            AGIFaltkod = string.Empty, ArAvdrag = false
        },
    ];
}
