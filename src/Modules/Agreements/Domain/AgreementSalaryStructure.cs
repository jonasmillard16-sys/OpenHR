using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

/// <summary>Lönestruktur per kollektivavtal med JSON-baserade kategorier och steg</summary>
public sealed class AgreementSalaryStructure
{
    public Guid Id { get; private set; }
    public CollectiveAgreementId AvtalsId { get; private set; }

    /// <summary>JSON: minimilön per befattningskategori, t.ex. {"Sjukskoterska": 28500, "Lakare": 45000}</summary>
    public string MinLonPerKategori { get; private set; } = "{}";

    /// <summary>JSON: lönesteg per erfarenhetsår, t.ex. [{"Ar": 0, "Belopp": 28500}, {"Ar": 5, "Belopp": 31000}]</summary>
    public string LoneSteg { get; private set; } = "[]";

    private AgreementSalaryStructure() { } // EF Core

    internal static AgreementSalaryStructure Skapa(
        CollectiveAgreementId avtalsId,
        string minLonPerKategori,
        string loneSteg)
    {
        return new AgreementSalaryStructure
        {
            Id = Guid.NewGuid(),
            AvtalsId = avtalsId,
            MinLonPerKategori = minLonPerKategori,
            LoneSteg = loneSteg
        };
    }
}
