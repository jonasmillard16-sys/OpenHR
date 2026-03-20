using RegionHR.Benefits.Domain;
using Xunit;

namespace RegionHR.Benefits.Tests;

public class EligibilityRuleTests
{
    [Fact]
    public void Skapa_SatterEgenskaperKorrekt()
    {
        var benefitId = Guid.NewGuid();
        var rule = EligibilityRule.Skapa(benefitId, "Friskvård — Tillsvidare", "AND");

        Assert.Equal(benefitId, rule.BenefitId);
        Assert.Equal("Friskvård — Tillsvidare", rule.Namn);
        Assert.Equal("AND", rule.Kombination);
        Assert.NotEqual(Guid.Empty, rule.Id);
        Assert.Empty(rule.Villkor);
    }

    [Fact]
    public void Skapa_OgiltigKombination_KastarFel()
    {
        Assert.Throws<ArgumentException>(() =>
            EligibilityRule.Skapa(Guid.NewGuid(), "Test", "XOR"));
    }

    [Fact]
    public void LaggTillVillkor_LaggerTillVillkor()
    {
        var rule = EligibilityRule.Skapa(Guid.NewGuid(), "Test", "AND");

        var condition = rule.LaggTillVillkor("AnstallningsForm", "IN", """["Tillsvidare"]""");

        Assert.Single(rule.Villkor);
        Assert.Equal("AnstallningsForm", condition.Falt);
        Assert.Equal("IN", condition.Operator);
    }

    [Fact]
    public void Utvardera_AND_AllaVillkorMaste()
    {
        var rule = EligibilityRule.Skapa(Guid.NewGuid(), "Test", "AND");
        rule.LaggTillVillkor("AnstallningsForm", "IN", """["Tillsvidare"]""");
        rule.LaggTillVillkor("Sysselsattningsgrad", "GE", """50""");

        var data = new Dictionary<string, string>
        {
            ["AnstallningsForm"] = "Tillsvidare",
            ["Sysselsattningsgrad"] = "100"
        };

        Assert.True(rule.Utvardera(data));
    }

    [Fact]
    public void Utvardera_AND_EnSomFelgerFalse()
    {
        var rule = EligibilityRule.Skapa(Guid.NewGuid(), "Test", "AND");
        rule.LaggTillVillkor("AnstallningsForm", "IN", """["Tillsvidare"]""");
        rule.LaggTillVillkor("Sysselsattningsgrad", "GE", """50""");

        var data = new Dictionary<string, string>
        {
            ["AnstallningsForm"] = "Vikariat",
            ["Sysselsattningsgrad"] = "100"
        };

        Assert.False(rule.Utvardera(data));
    }

    [Fact]
    public void Utvardera_OR_EnRackerForTrue()
    {
        var rule = EligibilityRule.Skapa(Guid.NewGuid(), "Test", "OR");
        rule.LaggTillVillkor("AnstallningsForm", "IN", """["Tillsvidare"]""");
        rule.LaggTillVillkor("Befattningskategori", "IN", """["Chef"]""");

        var data = new Dictionary<string, string>
        {
            ["AnstallningsForm"] = "Vikariat",
            ["Befattningskategori"] = "Chef"
        };

        Assert.True(rule.Utvardera(data));
    }

    [Fact]
    public void Utvardera_UtanVillkor_ReturnerarTrue()
    {
        var rule = EligibilityRule.Skapa(Guid.NewGuid(), "Alla", "AND");
        var data = new Dictionary<string, string> { ["X"] = "Y" };

        Assert.True(rule.Utvardera(data));
    }
}

public class EligibilityConditionTests
{
    [Fact]
    public void Skapa_OgiltigtFalt_KastarFel()
    {
        Assert.Throws<ArgumentException>(() =>
            EligibilityCondition.Skapa(Guid.NewGuid(), "OgiltigtFalt", "EQ", "test"));
    }

    [Fact]
    public void Skapa_OgiltigOperator_KastarFel()
    {
        Assert.Throws<ArgumentException>(() =>
            EligibilityCondition.Skapa(Guid.NewGuid(), "Alder", "LIKE", "test"));
    }

    [Fact]
    public void Utvardera_EQ_Matchar()
    {
        var condition = EligibilityCondition.Skapa(Guid.NewGuid(), "Befattningskategori", "EQ", """"Chef"""");
        var data = new Dictionary<string, string> { ["Befattningskategori"] = "Chef" };

        Assert.True(condition.Utvardera(data));
    }

    [Fact]
    public void Utvardera_GE_StorreEllerLika()
    {
        var condition = EligibilityCondition.Skapa(Guid.NewGuid(), "Sysselsattningsgrad", "GE", """50""");
        var data = new Dictionary<string, string> { ["Sysselsattningsgrad"] = "75" };

        Assert.True(condition.Utvardera(data));
    }

    [Fact]
    public void Utvardera_GE_UnderGransenFalse()
    {
        var condition = EligibilityCondition.Skapa(Guid.NewGuid(), "Sysselsattningsgrad", "GE", """50""");
        var data = new Dictionary<string, string> { ["Sysselsattningsgrad"] = "25" };

        Assert.False(condition.Utvardera(data));
    }

    [Fact]
    public void Utvardera_IN_Matchar()
    {
        var condition = EligibilityCondition.Skapa(Guid.NewGuid(), "AnstallningsForm", "IN", """["Tillsvidare","Vikariat"]""");
        var data = new Dictionary<string, string> { ["AnstallningsForm"] = "Tillsvidare" };

        Assert.True(condition.Utvardera(data));
    }

    [Fact]
    public void Utvardera_NOT_IN_InteMedIListan()
    {
        var condition = EligibilityCondition.Skapa(Guid.NewGuid(), "AnstallningsForm", "NOT_IN", """["Vikariat"]""");
        var data = new Dictionary<string, string> { ["AnstallningsForm"] = "Tillsvidare" };

        Assert.True(condition.Utvardera(data));
    }

    [Fact]
    public void Utvardera_BETWEEN_InomIntervall()
    {
        var condition = EligibilityCondition.Skapa(Guid.NewGuid(), "Alder", "BETWEEN", """[25,65]""");
        var data = new Dictionary<string, string> { ["Alder"] = "40" };

        Assert.True(condition.Utvardera(data));
    }

    [Fact]
    public void Utvardera_SaknarFalt_ReturnsFalse()
    {
        var condition = EligibilityCondition.Skapa(Guid.NewGuid(), "Alder", "GE", """25""");
        var data = new Dictionary<string, string> { ["AnstallningsForm"] = "Tillsvidare" };

        Assert.False(condition.Utvardera(data));
    }
}

public class BenefitEnrollmentTests
{
    [Fact]
    public void Skapa_SatterStatusTillPending()
    {
        var enrollment = BenefitEnrollment.Skapa(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1));

        Assert.Equal("Pending", enrollment.Status);
        Assert.NotEqual(Guid.Empty, enrollment.Id);
    }

    [Fact]
    public void Skapa_MedValdNiva_SparNiva()
    {
        var enrollment = BenefitEnrollment.Skapa(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1), "Premium");

        Assert.Equal("Premium", enrollment.ValdNiva);
    }

    [Fact]
    public void Aktivera_FranPending_SatterActive()
    {
        var enrollment = BenefitEnrollment.Skapa(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1));

        enrollment.Aktivera();

        Assert.Equal("Active", enrollment.Status);
    }

    [Fact]
    public void Aktivera_FranActive_KastarFel()
    {
        var enrollment = BenefitEnrollment.Skapa(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1));
        enrollment.Aktivera();

        Assert.Throws<InvalidOperationException>(() => enrollment.Aktivera());
    }

    [Fact]
    public void Avbryt_FranPending_SatterCancelled()
    {
        var enrollment = BenefitEnrollment.Skapa(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1));

        enrollment.Avbryt();

        Assert.Equal("Cancelled", enrollment.Status);
    }

    [Fact]
    public void Avbryt_FranActive_SatterCancelled()
    {
        var enrollment = BenefitEnrollment.Skapa(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1));
        enrollment.Aktivera();

        enrollment.Avbryt();

        Assert.Equal("Cancelled", enrollment.Status);
    }

    [Fact]
    public void Avbryt_RedanAvbruten_KastarFel()
    {
        var enrollment = BenefitEnrollment.Skapa(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1));
        enrollment.Avbryt();

        Assert.Throws<InvalidOperationException>(() => enrollment.Avbryt());
    }
}

public class LifeEventTests
{
    [Fact]
    public void Skapa_SatterEgenskaper()
    {
        var le = LifeEvent.Skapa("Nyanställning", "Nyanstallning", 30);

        Assert.Equal("Nyanställning", le.Namn);
        Assert.Equal("Nyanstallning", le.Typ);
        Assert.Equal(30, le.TidsFonsterDagar);
        Assert.NotEqual(Guid.Empty, le.Id);
    }

    [Fact]
    public void Skapa_OgiltigTyp_KastarFel()
    {
        Assert.Throws<ArgumentException>(() =>
            LifeEvent.Skapa("Test", "OgiltigTyp", 30));
    }

    [Fact]
    public void Skapa_NegativtTidsFonster_KastarFel()
    {
        Assert.Throws<ArgumentException>(() =>
            LifeEvent.Skapa("Test", "Nyanstallning", -1));
    }

    [Fact]
    public void ArInomTidsFonster_NollDagar_AlltidTrue()
    {
        var le = LifeEvent.Skapa("Avslut", "Avslut", 0);

        Assert.True(le.ArInomTidsFonster(new DateOnly(2020, 1, 1)));
    }

    [Fact]
    public void ArInomTidsFonster_InomFonster_True()
    {
        var le = LifeEvent.Skapa("Nytt barn", "BarnFott", 60);
        var datum = DateOnly.FromDateTime(DateTime.UtcNow);

        Assert.True(le.ArInomTidsFonster(datum));
    }

    [Fact]
    public void ArInomTidsFonster_UtanforFonster_False()
    {
        var le = LifeEvent.Skapa("Nytt barn", "BarnFott", 30);
        var datum = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60));

        Assert.False(le.ArInomTidsFonster(datum));
    }
}

public class LifeEventOccurrenceTests
{
    [Fact]
    public void Registrera_SatterStatusTillRegistered()
    {
        var occurrence = LifeEventOccurrence.Registrera(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 3, 1));

        Assert.Equal("Registered", occurrence.Status);
    }

    [Fact]
    public void Bearbeta_AndrarStatusTillProcessed()
    {
        var occurrence = LifeEventOccurrence.Registrera(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 3, 1));

        occurrence.Bearbeta();

        Assert.Equal("Processed", occurrence.Status);
    }
}

public class EnrollmentPeriodTests
{
    [Fact]
    public void Skapa_SatterStatusTillUpcoming()
    {
        var period = EnrollmentPeriod.Skapa("Öppet val 2026", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

        Assert.Equal("Upcoming", period.Status);
    }

    [Fact]
    public void Skapa_SlutDatumForeStartDatum_KastarFel()
    {
        Assert.Throws<ArgumentException>(() =>
            EnrollmentPeriod.Skapa("Test", new DateOnly(2026, 12, 1), new DateOnly(2026, 11, 1)));
    }

    [Fact]
    public void Oppna_SatterStatusTillOpen()
    {
        var period = EnrollmentPeriod.Skapa("Test", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

        period.Oppna();

        Assert.Equal("Open", period.Status);
    }

    [Fact]
    public void Stang_SatterStatusTillClosed()
    {
        var period = EnrollmentPeriod.Skapa("Test", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
        period.Oppna();

        period.Stang();

        Assert.Equal("Closed", period.Status);
    }

    [Fact]
    public void ArOppen_NarUpcoming_False()
    {
        var period = EnrollmentPeriod.Skapa("Test", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

        Assert.False(period.ArOppen(new DateOnly(2026, 11, 15)));
    }

    [Fact]
    public void ArOppen_NarOpenOchInomDatum_True()
    {
        var period = EnrollmentPeriod.Skapa("Test", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
        period.Oppna();

        Assert.True(period.ArOppen(new DateOnly(2026, 11, 15)));
    }
}

public class BenefitStatementTests
{
    [Fact]
    public void Generera_SatterEgenskaperKorrekt()
    {
        var statement = BenefitStatement.Generera(Guid.NewGuid(), 2026, """[{"Id":"abc"}]""", 15000m);

        Assert.Equal(2026, statement.Ar);
        Assert.Equal(15000m, statement.TotaltVarde);
        Assert.NotNull(statement.AktivaFormaner);
        Assert.NotEqual(Guid.Empty, statement.Id);
    }
}

public class BenefitTransactionTests
{
    [Fact]
    public void Skapa_SatterEgenskaperKorrekt()
    {
        var t = BenefitTransaction.Skapa(Guid.NewGuid(), Guid.NewGuid(), "Claim", 500m, new DateOnly(2026, 3, 1), "Simning");

        Assert.Equal("Claim", t.Typ);
        Assert.Equal(500m, t.Belopp);
        Assert.Equal("Simning", t.Beskrivning);
    }

    [Fact]
    public void Skapa_OgiltigTyp_KastarFel()
    {
        Assert.Throws<ArgumentException>(() =>
            BenefitTransaction.Skapa(Guid.NewGuid(), Guid.NewGuid(), "Invalid", 500m, new DateOnly(2026, 3, 1), "Test"));
    }

    [Fact]
    public void Skapa_NollBelopp_KastarFel()
    {
        Assert.Throws<ArgumentException>(() =>
            BenefitTransaction.Skapa(Guid.NewGuid(), Guid.NewGuid(), "Claim", 0m, new DateOnly(2026, 3, 1), "Test"));
    }

    [Fact]
    public void Skapa_NegativtBelopp_KastarFel()
    {
        Assert.Throws<ArgumentException>(() =>
            BenefitTransaction.Skapa(Guid.NewGuid(), Guid.NewGuid(), "Uttag", -100m, new DateOnly(2026, 3, 1), "Test"));
    }
}
