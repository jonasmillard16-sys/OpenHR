using RegionHR.Benefits.Domain;
using Xunit;

namespace RegionHR.Benefits.Tests;

public class BenefitTests
{
    [Fact]
    public void Skapa_SatterEgenskaperKorrekt()
    {
        var benefit = Benefit.Skapa("Friskvårdsbidrag", "Bidrag för friskvård", BenefitCategory.Friskvard, 5000m, 100m, false);

        Assert.Equal("Friskvårdsbidrag", benefit.Namn);
        Assert.Equal("Bidrag för friskvård", benefit.Beskrivning);
        Assert.Equal(BenefitCategory.Friskvard, benefit.Kategori);
        Assert.Equal(5000m, benefit.MaxBelopp);
        Assert.Equal(100m, benefit.ArbetsgivarAndel);
        Assert.Equal(0m, benefit.ArbetstagarAndel);
        Assert.False(benefit.ArSkattepliktig);
        Assert.True(benefit.ArAktiv);
        Assert.NotEqual(Guid.Empty, benefit.Id);
    }

    [Fact]
    public void Skapa_BeraknarArbetstagarAndelKorrekt()
    {
        var benefit = Benefit.Skapa("Tjänstebil", "Bil via arbetsgivaren", BenefitCategory.Tjanstebil, 8000m, 60m, true);

        Assert.Equal(60m, benefit.ArbetsgivarAndel);
        Assert.Equal(40m, benefit.ArbetstagarAndel);
    }

    [Fact]
    public void Inaktivera_SatterArAktivTillFalse()
    {
        var benefit = Benefit.Skapa("Pension", "Tjänstepension", BenefitCategory.Pension, 10000m, 100m, false);

        benefit.Inaktivera();

        Assert.False(benefit.ArAktiv);
    }

    [Fact]
    public void Aktivera_SatterArAktivTillTrue()
    {
        var benefit = Benefit.Skapa("Sjukvård", "Sjukvårdsförsäkring", BenefitCategory.Sjukvard, 3000m, 100m, true);
        benefit.Inaktivera();
        Assert.False(benefit.ArAktiv);

        benefit.Aktivera();

        Assert.True(benefit.ArAktiv);
    }

    [Fact]
    public void Skapa_MedEligibilityRegler_SparRegler()
    {
        var regler = """{"minAnstallningstid": "6 månader"}""";
        var benefit = Benefit.Skapa("Utbildning", "Vidareutbildning", BenefitCategory.Utbildning, 20000m, 100m, false, regler);

        Assert.Equal(regler, benefit.EligibilityRegler);
    }

    [Fact]
    public void Skapa_UtanEligibilityRegler_ArNull()
    {
        var benefit = Benefit.Skapa("Övrigt", "Övrig förmån", BenefitCategory.Ovrigt, 1000m, 50m, false);

        Assert.Null(benefit.EligibilityRegler);
    }
}

public class EmployeeBenefitTests
{
    private readonly Guid _anstallId = Guid.NewGuid();
    private readonly Guid _benefitId = Guid.NewGuid();

    [Fact]
    public void Anmala_SatterStatusTillAnsokt()
    {
        var enrollment = EmployeeBenefit.Anmala(_anstallId, _benefitId, new DateOnly(2026, 4, 1), 5000m);

        Assert.Equal(EnrollmentStatus.Ansokt, enrollment.Status);
        Assert.Equal(_anstallId, enrollment.AnstallId);
        Assert.Equal(_benefitId, enrollment.BenefitId);
        Assert.Equal(5000m, enrollment.ValtBelopp);
        Assert.NotEqual(Guid.Empty, enrollment.Id);
    }

    [Fact]
    public void Godkann_AndrarStatusTillAktiv()
    {
        var enrollment = EmployeeBenefit.Anmala(_anstallId, _benefitId, new DateOnly(2026, 4, 1), 3000m);

        enrollment.Godkann();

        Assert.Equal(EnrollmentStatus.Aktiv, enrollment.Status);
    }

    [Fact]
    public void Neka_AndrarStatusTillNekad()
    {
        var enrollment = EmployeeBenefit.Anmala(_anstallId, _benefitId, new DateOnly(2026, 4, 1), 3000m);

        enrollment.Neka();

        Assert.Equal(EnrollmentStatus.Nekad, enrollment.Status);
    }

    [Fact]
    public void Avsluta_SatterSlutDatumOchStatus()
    {
        var enrollment = EmployeeBenefit.Anmala(_anstallId, _benefitId, new DateOnly(2026, 4, 1), 5000m);
        enrollment.Godkann();

        var slutDatum = new DateOnly(2026, 12, 31);
        enrollment.Avsluta(slutDatum);

        Assert.Equal(EnrollmentStatus.Avslutad, enrollment.Status);
        Assert.Equal(slutDatum, enrollment.SlutDatum);
    }

    [Fact]
    public void Anmala_MedLivshandelse_SparAnledning()
    {
        var enrollment = EmployeeBenefit.Anmala(_anstallId, _benefitId, new DateOnly(2026, 4, 1), 5000m, "Barn fött");

        Assert.Equal("Barn fött", enrollment.LivshandardAnledning);
    }
}
