using RegionHR.Competence.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Competence.Tests;

public class CareerPathTests
{
    [Fact]
    public void Skapa_CreatesCareerPath_WithCorrectProperties()
    {
        var cp = CareerPath.Skapa("Test Path", "Vard", "En testbeskrivning");

        Assert.Equal("Test Path", cp.Namn);
        Assert.Equal("Vard", cp.Bransch);
        Assert.Equal("En testbeskrivning", cp.Beskrivning);
        Assert.NotEqual(default, cp.Id);
        Assert.Empty(cp.Steg);
    }

    [Fact]
    public void LaggTillSteg_AddsStepInOrder()
    {
        var cp = CareerPath.Skapa("Sjukskoterska", "Vard");

        var s1 = cp.LaggTillSteg("Grundutbildad", 1, 24);
        var s2 = cp.LaggTillSteg("Specialist", 2, 36);
        var s3 = cp.LaggTillSteg("Enhetschef", 3, 0);

        Assert.Equal(3, cp.Steg.Count);
        Assert.Equal(1, cp.Steg[0].Ordning);
        Assert.Equal(2, cp.Steg[1].Ordning);
        Assert.Equal(3, cp.Steg[2].Ordning);
        Assert.Equal("Grundutbildad", cp.Steg[0].Befattning);
        Assert.Equal(24, cp.Steg[0].TypiskTidManader);
    }

    [Fact]
    public void LaggTillSteg_SetsKravdaSkills()
    {
        var cp = CareerPath.Skapa("IT", "Tech");
        var step = cp.LaggTillSteg("Utvecklare", 1, 24, "[{\"Skill\":\"C#\",\"Niva\":3}]", 12);

        Assert.Equal("[{\"Skill\":\"C#\",\"Niva\":3}]", step.KravdaSkills);
        Assert.Equal(12, step.KravdErfarenhetManader);
    }

    [Fact]
    public void CareerPathStep_HasCorrectCareerPathId()
    {
        var cp = CareerPath.Skapa("Path", "Branch");
        var step = cp.LaggTillSteg("Step", 1, 12);

        Assert.Equal(cp.Id, step.CareerPathId);
    }
}

public class DevelopmentPlanTests
{
    [Fact]
    public void Skapa_CreatesDraftPlan()
    {
        var plan = DevelopmentPlan.Skapa(Guid.NewGuid(), "Senior Utvecklare",
            DateOnly.FromDateTime(DateTime.Today));

        Assert.Equal(DevelopmentPlanStatus.Draft, plan.Status);
        Assert.Equal("Senior Utvecklare", plan.MalRoll);
        Assert.Empty(plan.Milstolpar);
    }

    [Fact]
    public void Aktivera_ChangesStatusFromDraftToActive()
    {
        var plan = DevelopmentPlan.Skapa(Guid.NewGuid(), "Tech Lead",
            DateOnly.FromDateTime(DateTime.Today));

        plan.Aktivera();

        Assert.Equal(DevelopmentPlanStatus.Active, plan.Status);
    }

    [Fact]
    public void Aktivera_ThrowsIfNotDraft()
    {
        var plan = DevelopmentPlan.Skapa(Guid.NewGuid(), "Tech Lead",
            DateOnly.FromDateTime(DateTime.Today));
        plan.Aktivera();

        Assert.Throws<InvalidOperationException>(() => plan.Aktivera());
    }

    [Fact]
    public void Slutfor_ChangesStatusFromActiveToCompleted()
    {
        var plan = DevelopmentPlan.Skapa(Guid.NewGuid(), "Tech Lead",
            DateOnly.FromDateTime(DateTime.Today));
        plan.Aktivera();

        plan.Slutfor();

        Assert.Equal(DevelopmentPlanStatus.Completed, plan.Status);
    }

    [Fact]
    public void Slutfor_ThrowsIfNotActive()
    {
        var plan = DevelopmentPlan.Skapa(Guid.NewGuid(), "Tech Lead",
            DateOnly.FromDateTime(DateTime.Today));

        Assert.Throws<InvalidOperationException>(() => plan.Slutfor());
    }

    [Fact]
    public void LaggTillMilstolpe_AddsMilestone()
    {
        var plan = DevelopmentPlan.Skapa(Guid.NewGuid(), "Specialistsjukskoterska",
            DateOnly.FromDateTime(DateTime.Today));

        var milstolpe = plan.LaggTillMilstolpe("Genomfor HLR-kurs", "Kurs",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(3)));

        Assert.Single(plan.Milstolpar);
        Assert.Equal("Genomfor HLR-kurs", milstolpe.Beskrivning);
        Assert.Equal("Kurs", milstolpe.Typ);
        Assert.Equal(MilestoneStatus.Pending, milstolpe.Status);
    }

    [Fact]
    public void FullWorkflow_DraftActivateCompleteMilestones()
    {
        var plan = DevelopmentPlan.Skapa(Guid.NewGuid(), "Vardenhetschef",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddYears(2)));

        var m1 = plan.LaggTillMilstolpe("Ledarskapsutbildning", "Kurs");
        var m2 = plan.LaggTillMilstolpe("Certifiering i arbetsratt", "Certifiering");

        plan.Aktivera();
        Assert.Equal(DevelopmentPlanStatus.Active, plan.Status);

        m1.MarkeraPaborjad();
        Assert.Equal(MilestoneStatus.InProgress, m1.Status);

        m1.MarkeraKlar();
        Assert.Equal(MilestoneStatus.Completed, m1.Status);

        m2.MarkeraPaborjad();
        m2.MarkeraKlar();

        plan.Slutfor();
        Assert.Equal(DevelopmentPlanStatus.Completed, plan.Status);
    }
}

public class DevelopmentMilestoneTests
{
    [Fact]
    public void MarkeraPaborjad_ChangesStatusFromPendingToInProgress()
    {
        var plan = DevelopmentPlan.Skapa(Guid.NewGuid(), "Mal",
            DateOnly.FromDateTime(DateTime.Today));
        var m = plan.LaggTillMilstolpe("Test", "Skill");

        m.MarkeraPaborjad();

        Assert.Equal(MilestoneStatus.InProgress, m.Status);
    }

    [Fact]
    public void MarkeraPaborjad_ThrowsIfNotPending()
    {
        var plan = DevelopmentPlan.Skapa(Guid.NewGuid(), "Mal",
            DateOnly.FromDateTime(DateTime.Today));
        var m = plan.LaggTillMilstolpe("Test", "Skill");
        m.MarkeraPaborjad();

        Assert.Throws<InvalidOperationException>(() => m.MarkeraPaborjad());
    }

    [Fact]
    public void MarkeraKlar_WorksFromInProgress()
    {
        var plan = DevelopmentPlan.Skapa(Guid.NewGuid(), "Mal",
            DateOnly.FromDateTime(DateTime.Today));
        var m = plan.LaggTillMilstolpe("Test", "Skill");
        m.MarkeraPaborjad();

        m.MarkeraKlar();

        Assert.Equal(MilestoneStatus.Completed, m.Status);
    }

    [Fact]
    public void MarkeraKlar_ThrowsIfAlreadyCompleted()
    {
        var plan = DevelopmentPlan.Skapa(Guid.NewGuid(), "Mal",
            DateOnly.FromDateTime(DateTime.Today));
        var m = plan.LaggTillMilstolpe("Test", "Skill");
        m.MarkeraPaborjad();
        m.MarkeraKlar();

        Assert.Throws<InvalidOperationException>(() => m.MarkeraKlar());
    }
}

public class InternalOpportunityTests
{
    [Fact]
    public void Skapa_CreatesDraftOpportunity()
    {
        var opp = InternalOpportunity.Skapa("Projekt", "Digitalisering", Guid.NewGuid());

        Assert.Equal(OpportunityStatus.Draft, opp.Status);
        Assert.Equal("Projekt", opp.Typ);
        Assert.Equal("Digitalisering", opp.Titel);
    }

    [Fact]
    public void Publicera_ChangesStatusFromDraftToPublished()
    {
        var opp = InternalOpportunity.Skapa("Roll", "Systemutvecklare", Guid.NewGuid());

        opp.Publicera();

        Assert.Equal(OpportunityStatus.Published, opp.Status);
    }

    [Fact]
    public void Publicera_ThrowsIfNotDraft()
    {
        var opp = InternalOpportunity.Skapa("Roll", "Test", Guid.NewGuid());
        opp.Publicera();

        Assert.Throws<InvalidOperationException>(() => opp.Publicera());
    }

    [Fact]
    public void Stang_ChangesStatusFromPublishedToClosed()
    {
        var opp = InternalOpportunity.Skapa("Gig", "Korttidsuppdrag", Guid.NewGuid());
        opp.Publicera();

        opp.Stang();

        Assert.Equal(OpportunityStatus.Closed, opp.Status);
    }

    [Fact]
    public void Stang_ThrowsIfNotPublished()
    {
        var opp = InternalOpportunity.Skapa("Gig", "Test", Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => opp.Stang());
    }

    [Fact]
    public void Tillsatt_ChangesStatusToFilled()
    {
        var opp = InternalOpportunity.Skapa("Roll", "Avdelningschef", Guid.NewGuid());
        opp.Publicera();

        opp.Tillsatt();

        Assert.Equal(OpportunityStatus.Filled, opp.Status);
    }

    [Fact]
    public void Tillsatt_WorksFromClosed()
    {
        var opp = InternalOpportunity.Skapa("Roll", "Avdelningschef", Guid.NewGuid());
        opp.Publicera();
        opp.Stang();

        opp.Tillsatt();

        Assert.Equal(OpportunityStatus.Filled, opp.Status);
    }

    [Fact]
    public void Tillsatt_ThrowsIfDraft()
    {
        var opp = InternalOpportunity.Skapa("Roll", "Test", Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => opp.Tillsatt());
    }

    [Fact]
    public void StatusFlow_DraftToPublishedToClosedToFilled()
    {
        var opp = InternalOpportunity.Skapa("Projekt", "Ny portal", Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(6)));

        Assert.Equal(OpportunityStatus.Draft, opp.Status);
        opp.Publicera();
        Assert.Equal(OpportunityStatus.Published, opp.Status);
        opp.Stang();
        Assert.Equal(OpportunityStatus.Closed, opp.Status);
        opp.Tillsatt();
        Assert.Equal(OpportunityStatus.Filled, opp.Status);
    }
}

public class InferredSkillTests
{
    [Fact]
    public void Skapa_CreatesUnconfirmedInferredSkill()
    {
        var inf = InferredSkill.Skapa(Guid.NewGuid(), Guid.NewGuid(), "Befattning", 75);

        Assert.Equal("Befattning", inf.Kalla);
        Assert.Equal(75, inf.Konfidens);
        Assert.False(inf.ArBekraftad);
    }

    [Fact]
    public void Skapa_ThrowsForInvalidKonfidens()
    {
        Assert.Throws<ArgumentException>(() =>
            InferredSkill.Skapa(Guid.NewGuid(), Guid.NewGuid(), "Kurs", -1));

        Assert.Throws<ArgumentException>(() =>
            InferredSkill.Skapa(Guid.NewGuid(), Guid.NewGuid(), "Kurs", 101));
    }

    [Fact]
    public void Bekrafta_SetsArBekraftadAndKonfidens100()
    {
        var inf = InferredSkill.Skapa(Guid.NewGuid(), Guid.NewGuid(), "Erfarenhet", 60);

        inf.Bekrafta();

        Assert.True(inf.ArBekraftad);
        Assert.Equal(100, inf.Konfidens);
    }
}

public class OpportunityApplicationTests
{
    [Fact]
    public void Skapa_CreatesSubmittedApplication()
    {
        var oppId = InternalOpportunityId.New();
        var app = OpportunityApplication.Skapa(oppId, Guid.NewGuid(), "Jag ar intresserad", 85);

        Assert.Equal(ApplicationStatus.Submitted, app.Status);
        Assert.Equal(85, app.MatchScore);
        Assert.Equal("Jag ar intresserad", app.Motivering);
    }

    [Fact]
    public void Skapa_ThrowsForInvalidMatchScore()
    {
        Assert.Throws<ArgumentException>(() =>
            OpportunityApplication.Skapa(InternalOpportunityId.New(), Guid.NewGuid(), null, -1));

        Assert.Throws<ArgumentException>(() =>
            OpportunityApplication.Skapa(InternalOpportunityId.New(), Guid.NewGuid(), null, 101));
    }

    [Fact]
    public void Skapa_SetsTimestamp()
    {
        var app = OpportunityApplication.Skapa(InternalOpportunityId.New(), Guid.NewGuid(), null, 50);

        Assert.True(app.SkapadVid > DateTime.MinValue);
        Assert.True(app.SkapadVid <= DateTime.UtcNow);
    }
}

public class SkillCategoryEntityTests
{
    [Fact]
    public void Skapa_CreatesCategory()
    {
        var cat = SkillCategoryEntity.Skapa("Klinisk", "Kliniska vardkompetenser");

        Assert.Equal("Klinisk", cat.Namn);
        Assert.Equal("Kliniska vardkompetenser", cat.Beskrivning);
        Assert.NotEqual(Guid.Empty, cat.Id);
    }

    [Fact]
    public void Skapa_WithoutBeskrivning()
    {
        var cat = SkillCategoryEntity.Skapa("Teknisk");

        Assert.Equal("Teknisk", cat.Namn);
        Assert.Null(cat.Beskrivning);
    }
}

public class SkillRelationTests
{
    [Fact]
    public void Skapa_CreatesRelation()
    {
        var from = Guid.NewGuid();
        var to = Guid.NewGuid();
        var rel = SkillRelation.Skapa(from, to, "Prerequisite");

        Assert.Equal(from, rel.FranSkillId);
        Assert.Equal(to, rel.TillSkillId);
        Assert.Equal("Prerequisite", rel.Typ);
    }

    [Fact]
    public void Skapa_ThrowsForEmptyTyp()
    {
        Assert.Throws<ArgumentException>(() =>
            SkillRelation.Skapa(Guid.NewGuid(), Guid.NewGuid(), ""));
    }
}

public class MentorRelationTests
{
    [Fact]
    public void Skapa_CreatesActiveRelation()
    {
        var rel = MentorRelation.Skapa(Guid.NewGuid(), Guid.NewGuid(), "Ledarskap",
            DateOnly.FromDateTime(DateTime.Today), 14);

        Assert.Equal("Active", rel.Status);
        Assert.Equal("Ledarskap", rel.FokusOmrade);
        Assert.Equal(14, rel.MotesFrekvensDagar);
    }
}

public class SkillEndorsementTests
{
    [Fact]
    public void Skapa_CreatesEndorsement()
    {
        var skillId = Guid.NewGuid();
        var anstallId = Guid.NewGuid();
        var bekraftadAv = Guid.NewGuid();

        var endorsement = SkillEndorsement.Skapa(skillId, anstallId, bekraftadAv);

        Assert.Equal(skillId, endorsement.SkillId);
        Assert.Equal(anstallId, endorsement.AnstallId);
        Assert.Equal(bekraftadAv, endorsement.BekraftadAv);
        Assert.True(endorsement.Datum <= DateTime.UtcNow);
    }
}

public class MatchScoreCalculationTests
{
    [Fact]
    public void MatchScore_RangeValidation()
    {
        // Valid scores
        var app1 = OpportunityApplication.Skapa(InternalOpportunityId.New(), Guid.NewGuid(), null, 0);
        Assert.Equal(0, app1.MatchScore);

        var app2 = OpportunityApplication.Skapa(InternalOpportunityId.New(), Guid.NewGuid(), null, 100);
        Assert.Equal(100, app2.MatchScore);

        var app3 = OpportunityApplication.Skapa(InternalOpportunityId.New(), Guid.NewGuid(), null, 50);
        Assert.Equal(50, app3.MatchScore);
    }
}
