using Xunit;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Tests;

public class CollectiveAgreementTests
{
    [Fact]
    public void Skapa_SkaparAvtalMedKorrekta_Egenskaper()
    {
        // Arrange & Act
        var avtal = CollectiveAgreement.Skapa(
            "AB 2025", "SKR och Kommunal", new DateOnly(2025, 4, 1), IndustrySector.KommunRegion);

        // Assert
        Assert.Equal("AB 2025", avtal.Namn);
        Assert.Equal("SKR och Kommunal", avtal.Parter);
        Assert.Equal(new DateOnly(2025, 4, 1), avtal.GiltigFran);
        Assert.Null(avtal.GiltigTill);
        Assert.Equal(IndustrySector.KommunRegion, avtal.Bransch);
        Assert.Equal(AgreementStatus.Aktivt, avtal.Status);
        Assert.NotEqual(default, avtal.Id);
    }

    [Fact]
    public void LaggTillOBSats_LaggsIKollektion()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);

        avtal.LaggTillOBSats(OBCategory.VardagKvall, 126.50m, new DateOnly(2025, 4, 1));
        avtal.LaggTillOBSats(OBCategory.VardagNatt, 152.00m, new DateOnly(2025, 4, 1));

        Assert.Equal(2, avtal.OBSatser.Count);
        Assert.Equal(126.50m, avtal.OBSatser[0].Belopp);
        Assert.Equal(OBCategory.VardagKvall, avtal.OBSatser[0].Tidstyp);
    }

    [Fact]
    public void HamtaOBSats_ReturnerarKorrektBelopp()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);
        avtal.LaggTillOBSats(OBCategory.VardagKvall, 126.50m, new DateOnly(2025, 4, 1));
        avtal.LaggTillOBSats(OBCategory.VardagNatt, 152.00m, new DateOnly(2025, 4, 1));
        avtal.LaggTillOBSats(OBCategory.Helg, 89.00m, new DateOnly(2025, 4, 1));

        Assert.Equal(126.50m, avtal.HamtaOBSats(OBCategory.VardagKvall, new DateOnly(2025, 6, 1)));
        Assert.Equal(152.00m, avtal.HamtaOBSats(OBCategory.VardagNatt, new DateOnly(2025, 6, 1)));
        Assert.Equal(89.00m, avtal.HamtaOBSats(OBCategory.Helg, new DateOnly(2025, 6, 1)));
    }

    [Fact]
    public void HamtaOBSats_ReturnerarNoll_ForSaknadKategori()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);
        avtal.LaggTillOBSats(OBCategory.VardagKvall, 126.50m, new DateOnly(2025, 4, 1));

        Assert.Equal(0m, avtal.HamtaOBSats(OBCategory.Storhelg, new DateOnly(2025, 6, 1)));
    }

    [Fact]
    public void HamtaOBSats_ReturnerarNoll_ForDatumUtanforGiltighet()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);
        avtal.LaggTillOBSats(OBCategory.VardagKvall, 126.50m, new DateOnly(2025, 4, 1), new DateOnly(2025, 12, 31));

        Assert.Equal(0m, avtal.HamtaOBSats(OBCategory.VardagKvall, new DateOnly(2024, 1, 1)));
        Assert.Equal(0m, avtal.HamtaOBSats(OBCategory.VardagKvall, new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void LaggTillOvertidsRegel_LaggsIKollektion()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);

        avtal.LaggTillOvertidsRegel(1.0m, 1.8m, 200m);

        Assert.Single(avtal.OvertidsRegler);
        Assert.Equal(1.8m, avtal.OvertidsRegler[0].Multiplikator);
        Assert.Equal(200m, avtal.OvertidsRegler[0].MaxPerAr);
    }

    [Fact]
    public void LaggTillSemesterRegel_LaggsIKollektion()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);

        avtal.LaggTillSemesterRegel(25, 31, 32);

        Assert.Single(avtal.SemesterRegler);
        Assert.Equal(25, avtal.SemesterRegler[0].BasDagar);
        Assert.Equal(31, avtal.SemesterRegler[0].ExtraDagarVid40);
        Assert.Equal(32, avtal.SemesterRegler[0].ExtraDagarVid50);
    }

    [Fact]
    public void LaggTillPensionsRegel_LaggsIKollektion()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);

        avtal.LaggTillPensionsRegel(PensionType.AKAPKR, 6.0m, 31.5m, 599500m, "{\"IBB\":599500}");

        Assert.Single(avtal.PensionsRegler);
        Assert.Equal(PensionType.AKAPKR, avtal.PensionsRegler[0].PensionsTyp);
        Assert.Equal(6.0m, avtal.PensionsRegler[0].SatsUnderTak);
        Assert.Equal(31.5m, avtal.PensionsRegler[0].SatsOverTak);
    }

    [Fact]
    public void LaggTillViloRegel_LaggsIKollektion()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);

        avtal.LaggTillViloRegel(11m, 36m, 0.5m);

        Assert.Single(avtal.ViloRegler);
        Assert.Equal(11m, avtal.ViloRegler[0].MinDygnsvila);
    }

    [Fact]
    public void LaggTillArbetstidsRegel_LaggsIKollektion()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);

        avtal.LaggTillArbetstidsRegel(38.25m, "{\"MaxFlexSaldo\":40}");

        Assert.Single(avtal.ArbetstidsRegler);
        Assert.Equal(38.25m, avtal.ArbetstidsRegler[0].NormalTimmarPerVecka);
    }

    [Fact]
    public void LaggTillUppságningsRegel_LaggsIKollektion()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);

        avtal.LaggTillUppságningsRegel(0, 1);
        avtal.LaggTillUppságningsRegel(24, 2);
        avtal.LaggTillUppságningsRegel(48, 3);

        Assert.Equal(3, avtal.UppságningsRegler.Count);
    }

    [Fact]
    public void LaggTillForsakringspaket_LaggsIKollektion()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);

        avtal.LaggTillForsakringspaket("{\"Belopp\":285000}", "{}", "{}", "{}", "{}");

        Assert.Single(avtal.ForsakringsRegler);
    }

    [Fact]
    public void LaggTillLonestruktur_LaggsIKollektion()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.KommunRegion);

        avtal.LaggTillLonestruktur("{\"Sjukskoterska\":28500}", "[]");

        Assert.Single(avtal.LonestrukturRegler);
    }

    [Fact]
    public void LaggTillPrivatErsattningsPlan_LaggsIKollektion()
    {
        var avtal = CollectiveAgreement.Skapa("IT-avtal", "Almega", new DateOnly(2025, 1, 1), IndustrySector.ITTelekom);

        avtal.LaggTillPrivatErsattningsPlan("{\"MaxProcent\":15}", "{}", "{}", "{}");

        Assert.Single(avtal.PrivatErsattningsPlaner);
    }

    [Fact]
    public void CollectiveAgreementId_New_SkaparUnikaId()
    {
        var id1 = CollectiveAgreementId.New();
        var id2 = CollectiveAgreementId.New();

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void CollectiveAgreementId_From_AterSkaparId()
    {
        var guid = Guid.NewGuid();
        var id = CollectiveAgreementId.From(guid);

        Assert.Equal(guid, id.Value);
    }
}
