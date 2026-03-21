using RegionHR.Core.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;
using RegionHR.Scheduling.Domain;
using RegionHR.Recruitment.Domain;
using RegionHR.Travel.Domain;
using RegionHR.SalaryReview.Domain;
using RegionHR.CaseManagement.Domain;
using RegionHR.Compensation.Domain;
using RegionHR.Benefits.Domain;
using RegionHR.Platform.Domain;
using RegionHR.Performance.Domain;
using RegionHR.Analytics.Domain;

namespace RegionHR.Api;

public static class DevDataSeeder
{
    public static void SeedDevData(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RegionHRDbContext>();

        // Organisation
        var start = new DateOnly(2020, 1, 1);
        var region = OrganizationUnit.Skapa("Region Mellansverige", OrganizationUnitType.Region, "1000", start);
        var forvaltning = OrganizationUnit.Skapa("Hälso- och sjukvårdsförvaltningen", OrganizationUnitType.Forvaltning, "2000", start, region.Id);
        var verksamhet = OrganizationUnit.Skapa("Akutsjukvård", OrganizationUnitType.Verksamhet, "2100", start, forvaltning.Id);
        var enhetCIVA = OrganizationUnit.Skapa("CIVA - Centralintensiven", OrganizationUnitType.Enhet, "2110", start, verksamhet.Id);
        var enhetAkuten = OrganizationUnit.Skapa("Akutmottagningen", OrganizationUnitType.Enhet, "2120", start, verksamhet.Id);
        var forvaltningHR = OrganizationUnit.Skapa("HR-förvaltningen", OrganizationUnitType.Forvaltning, "3000", start, region.Id);
        db.OrganizationUnits.AddRange(region, forvaltning, verksamhet, enhetCIVA, enhetAkuten, forvaltningHR);

        // Anställda (Luhn-validerade personnummer)
        var anna = Employee.Skapa(new Personnummer("198505150006"), "Anna", "Svensson");
        anna.UppdateraKontaktuppgifter("anna.svensson@region.se", "070-1234567", null);
        anna.UppdateraSkatteuppgifter(30, 1, "Västerås", 32.41m, false, null);

        var erik = Employee.Skapa(new Personnummer("199203120002"), "Erik", "Johansson");
        erik.UppdateraKontaktuppgifter("erik.johansson@region.se", "070-2345678", null);
        erik.UppdateraSkatteuppgifter(30, 1, "Stockholm", 30.44m, true, 1.02m);

        var maria = Employee.Skapa(new Personnummer("197811230007"), "Maria", "Andersson");
        maria.UppdateraKontaktuppgifter("maria.andersson@region.se", "070-3456789", null);
        maria.UppdateraSkatteuppgifter(33, 1, "Uppsala", 31.89m, false, null);

        var lars = Employee.Skapa(new Personnummer("200201140001"), "Lars", "Nilsson");
        lars.UppdateraKontaktuppgifter("lars.nilsson@region.se", "070-4567890", null);
        lars.UppdateraSkatteuppgifter(30, 1, "Västerås", 32.41m, false, null);

        var karin = Employee.Skapa(new Personnummer("196509070006"), "Karin", "Lindberg");
        karin.UppdateraKontaktuppgifter("karin.lindberg@region.se", "070-5678901", null);
        karin.UppdateraSkatteuppgifter(30, 1, "Västerås", 32.41m, false, null);

        db.Employees.AddRange(anna, erik, maria, lars, karin);

        // Anställningar
        anna.LaggTillAnstallning(enhetCIVA.Id, EmploymentType.Tillsvidare, CollectiveAgreementType.AB,
            Money.SEK(38500m), new Percentage(100), new DateOnly(2015, 3, 1), null, "222");
        erik.LaggTillAnstallning(enhetCIVA.Id, EmploymentType.Tillsvidare, CollectiveAgreementType.AB,
            Money.SEK(35000m), new Percentage(100), new DateOnly(2020, 8, 15), null, "222");
        maria.LaggTillAnstallning(enhetAkuten.Id, EmploymentType.Tillsvidare, CollectiveAgreementType.HOK,
            Money.SEK(42000m), new Percentage(80), new DateOnly(2010, 1, 1), null, "321");
        lars.LaggTillAnstallning(enhetCIVA.Id, EmploymentType.SAVA, CollectiveAgreementType.AB,
            Money.SEK(31000m), new Percentage(75), new DateOnly(2025, 6, 1), new DateOnly(2026, 5, 31), "222");
        karin.LaggTillAnstallning(forvaltningHR.Id, EmploymentType.Tillsvidare, CollectiveAgreementType.AB,
            Money.SEK(45000m), new Percentage(100), new DateOnly(2005, 9, 1), null, "400");

        // Vakans
        var vakans = Vacancy.Skapa(enhetCIVA.Id, "Sjuksköterska, natt",
            "Vi söker en erfaren sjuksköterska till CIVA nattjour. Krav: legitimation, IVA-erfarenhet.",
            EmploymentType.Tillsvidare, new DateOnly(2026, 4, 30));
        vakans.Publicera(true, true);
        vakans.TaEmotAnsokan("Sofia Berg", "sofia.berg@email.se", null);
        vakans.TaEmotAnsokan("Johan Ek", "johan.ek@email.se", null);

        var vakans2 = Vacancy.Skapa(enhetAkuten.Id, "Undersköterska, vikariat",
            "Vikariat som undersköterska på akutmottagningen. Sommarperiod juni-augusti.",
            EmploymentType.Vikariat, new DateOnly(2026, 05, 15));
        vakans2.Publicera(true, false);
        db.Vacancies.AddRange(vakans, vakans2);

        // Resekrav
        var resa = TravelClaim.Skapa(anna.Id, "Utbildning palliativ vård, Göteborg", new DateOnly(2026, 3, 10));
        resa.SattTraktamente(2, 1);
        resa.SattMilersattning(45m);
        resa.LaggTillUtlagg("Hotell", Money.SEK(1250m));
        resa.SkickaIn();
        db.TravelClaims.Add(resa);

        // Löneöversyn
        var runda = SalaryReviewRound.Skapa("Löneöversyn 2026 AB", 2026, CollectiveAgreementType.AB,
            Money.SEK(500000m), new DateOnly(2026, 4, 1));
        runda.LaggTillForslag(anna.Id, Money.SEK(38500m), Money.SEK(39800m), "Hög prestation, utökad CIVA-kompetens");
        runda.LaggTillForslag(erik.Id, Money.SEK(35000m), Money.SEK(36200m), "Bra utveckling, mentor för nyanställda");
        db.SalaryReviewRounds.Add(runda);

        // Frånvaroärende
        var arende = Case.SkapaFranvaroarende(maria.Id, AbsenceType.Semester,
            new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 25), "Sommarsemester");
        db.Cases.Add(arende);

        // Schema
        var schema = Schedule.SkapaGrundschema(enhetCIVA.Id, "CIVA Grundschema V12-16", new DateOnly(2026, 3, 16), 4);
        var today = DateOnly.FromDateTime(DateTime.Today);
        schema.LaggTillPass(anna.Id, today, ShiftType.Dag, new TimeOnly(7, 0), new TimeOnly(16, 0), TimeSpan.FromMinutes(60));
        schema.LaggTillPass(erik.Id, today, ShiftType.Kvall, new TimeOnly(15, 0), new TimeOnly(22, 0), TimeSpan.FromMinutes(30));
        schema.LaggTillPass(lars.Id, today, ShiftType.Natt, new TimeOnly(21, 0), new TimeOnly(7, 0), TimeSpan.FromMinutes(45));
        schema.LaggTillPass(anna.Id, today.AddDays(1), ShiftType.Dag, new TimeOnly(7, 0), new TimeOnly(16, 0), TimeSpan.FromMinutes(60));
        schema.LaggTillPass(maria.Id, today.AddDays(1), ShiftType.Kvall, new TimeOnly(15, 0), new TimeOnly(22, 0), TimeSpan.FromMinutes(30));
        schema.Publicera();
        db.Schedules.Add(schema);

        // Rehabiliteringsärende
        var rehab = HalsoSAM.Domain.RehabCase.Skapa(maria.Id, HalsoSAM.Domain.RehabTrigger.SexTillfallenTolvManader);
        rehab.TilldelaArendeagare(karin.Id);
        rehab.LaggTillAnteckning("Första samtalet genomfört. Medarbetaren upplever hög arbetsbelastning.", karin.Id);
        db.RehabCases.Add(rehab);

        // Compensation Suite — Band
        var bandSsk = new CompensationBand
        {
            Befattningskategori = "Sjukskoterska",
            Min = 32000m, Mal = 38500m, Max = 45000m,
            Steg1Min = 32000m, Steg1Max = 34500m,
            Steg2Min = 34500m, Steg2Max = 38500m,
            Steg3Min = 38500m, Steg3Max = 42000m,
            Steg4Min = 42000m, Steg4Max = 45000m
        };
        var bandLak = new CompensationBand
        {
            Befattningskategori = "Lakare",
            Min = 55000m, Mal = 62000m, Max = 72000m,
            Steg1Min = 55000m, Steg1Max = 58000m,
            Steg2Min = 58000m, Steg2Max = 62000m,
            Steg3Min = 62000m, Steg3Max = 67000m,
            Steg4Min = 67000m, Steg4Max = 72000m
        };
        db.CompensationBands.AddRange(bandSsk, bandLak);

        // Compensation Suite — Plan
        var compPlan = CompensationPlan.Skapa(
            "Lonerevision 2026", new DateOnly(2026, 4, 1), new DateOnly(2026, 12, 31), 2_500_000m);
        compPlan.Aktivera();
        compPlan.LaggTillRiktlinje(new CompensationGuideline
        {
            CompensationPlanId = compPlan.Id,
            PrestationsNiva = "Uppfyller forvantningar",
            RekommenderadHojningProcent = 2.5m,
            MaxHojningProcent = 3.5m
        });
        compPlan.LaggTillRiktlinje(new CompensationGuideline
        {
            CompensationPlanId = compPlan.Id,
            PrestationsNiva = "Overstiger forvantningar",
            RekommenderadHojningProcent = 4.0m,
            MaxHojningProcent = 6.0m
        });
        db.CompensationPlans.Add(compPlan);

        // ============================================================
        // Benefits Engine (Phase B3)
        // ============================================================

        // Seed Benefits (the existing entity) for the engine to reference
        var friskvard = Benefit.Skapa("Friskvårdsbidrag", "Bidrag för friskvårdsaktiviteter, max 5 000 kr/år", BenefitCategory.Friskvard, 5000m, 100m, false);
        var tjanstebil = Benefit.Skapa("Tjänstebil", "Förmånsbil via arbetsgivaren", BenefitCategory.Tjanstebil, 8000m, 60m, true);
        var sjukvard = Benefit.Skapa("Sjukvårdsförsäkring", "Privat sjukvårdsförsäkring", BenefitCategory.Sjukvard, 3500m, 100m, false);
        db.Benefits.AddRange(friskvard, tjanstebil, sjukvard);

        // LifeEvents (3 seeded)
        var leNyanstallning = LifeEvent.Skapa("Nyanställning", "Nyanstallning", 30, """["ValFriskvard","ValSjukvard"]""");
        var leBarnFott = LifeEvent.Skapa("Barn fött", "BarnFott", 60, """["AnpassaForsakring","ExtraLedighet"]""");
        var leAvslut = LifeEvent.Skapa("Avslut av anställning", "Avslut", 0, """["AvslutaAllaFormaner"]""");
        db.LifeEvents.AddRange(leNyanstallning, leBarnFott, leAvslut);

        // EligibilityRules (2 rules with conditions)
        var regelFriskvard = EligibilityRule.Skapa(friskvard.Id, "Friskvård — Tillsvidare + 50%", "AND");
        regelFriskvard.LaggTillVillkor("AnstallningsForm", "IN", """["Tillsvidare"]""");
        regelFriskvard.LaggTillVillkor("Sysselsattningsgrad", "GE", """50""");
        db.EligibilityRules.Add(regelFriskvard);

        var regelTjanstebil = EligibilityRule.Skapa(tjanstebil.Id, "Tjänstebil — Chef + Privat avtal", "AND");
        regelTjanstebil.LaggTillVillkor("Befattningskategori", "IN", """["Chef","Verksamhetschef"]""");
        regelTjanstebil.LaggTillVillkor("CollectiveAgreement", "EQ", """"Privat"""");
        db.EligibilityRules.Add(regelTjanstebil);

        // EnrollmentPeriod (Öppet val 2026)
        var oppetVal = EnrollmentPeriod.Skapa("Öppet val 2026", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30),
            System.Text.Json.JsonSerializer.Serialize(new[] { friskvard.Id, tjanstebil.Id, sjukvard.Id }));
        oppetVal.Oppna();
        db.EnrollmentPeriods.Add(oppetVal);

        // BenefitEnrollments for seeded employees
        var enrollAnna = BenefitEnrollment.Skapa(anna.Id.Value, friskvard.Id, new DateOnly(2026, 1, 1), "Standard");
        enrollAnna.Aktivera();
        var enrollErik = BenefitEnrollment.Skapa(erik.Id.Value, sjukvard.Id, new DateOnly(2026, 1, 1), "Grund");
        enrollErik.Aktivera();
        var enrollMaria = BenefitEnrollment.Skapa(maria.Id.Value, friskvard.Id, new DateOnly(2026, 3, 1));
        db.BenefitEnrollments.AddRange(enrollAnna, enrollErik, enrollMaria);

        // ============================================================
        // Marketplace — Extensions (Phase C Layer 3)
        // ============================================================

        var extUtrustning = Extension.Skapa(
            "openhr-utrustning",
            "1.0.0",
            "OpenHR Community",
            "Utrustningshantering - spara vilken utrustning (dator, telefon, passerkort) som tilldelats medarbetare.",
            ExtensionTyp.CustomObject,
            "AGPL-3.0",
            ">=2.0.0",
            """{"customObjects":["utrustning.json"],"workflows":[],"reports":[]}""");

        var extNyckelkvittens = Extension.Skapa(
            "openhr-nyckelkvittens",
            "1.0.0",
            "OpenHR Community",
            "Nyckelkvittens-workflow - automatiserat flode for utlamning och retur av nycklar.",
            ExtensionTyp.Workflow,
            "AGPL-3.0",
            ">=2.0.0",
            """{"customObjects":[],"workflows":["nyckelkvittens.json"],"reports":[]}""");

        db.Extensions.AddRange(extUtrustning, extNyckelkvittens);

        var installUtrustning = ExtensionInstallation.Installera(extUtrustning.Id, "1.0.0");
        db.ExtensionInstallations.Add(installUtrustning);

        // ============================================================
        // Manager Effectiveness
        // ============================================================

        // 1:1 Meetings
        var meeting1 = OneOnOneMeeting.Skapa(maria.Id.Value, anna.Id.Value, DateTime.UtcNow.AddDays(-14), "Utvecklingssamtal Q1");
        meeting1.Genomfor("Bra dialog om karriärmål. Anna vill fördjupa IVA-kompetens. Överens om utbildningsplan.");
        var meeting2 = OneOnOneMeeting.Skapa(maria.Id.Value, erik.Id.Value, DateTime.UtcNow.AddDays(-7), "Uppföljning mentorskap");
        meeting2.Genomfor("Erik trivs med mentorrollen. Diskuterade arbetsbelastning — allt ser bra ut.");
        var meeting3 = OneOnOneMeeting.Skapa(maria.Id.Value, lars.Id.Value, DateTime.UtcNow.AddDays(3), "Introduktionsuppföljning");
        var meeting4 = OneOnOneMeeting.Skapa(maria.Id.Value, anna.Id.Value, DateTime.UtcNow.AddDays(14), "Halvårsuppföljning");
        db.OneOnOneMeetings.AddRange(meeting1, meeting2, meeting3, meeting4);

        // Action Items
        var action1 = MeetingActionItem.Skapa(meeting1.Id, "Boka IVA-utbildning hos Karolinska", anna.Id.Value, new DateOnly(2026, 5, 1));
        var action2 = MeetingActionItem.Skapa(meeting1.Id, "Uppdatera utvecklingsplan i systemet", maria.Id.Value, new DateOnly(2026, 4, 15));
        action2.Paborja();
        var action3 = MeetingActionItem.Skapa(meeting2.Id, "Dokumentera mentorskapserfarenheter", erik.Id.Value, new DateOnly(2026, 6, 30));
        db.MeetingActionItems.AddRange(action1, action2, action3);

        // Manager Scorecard
        var scorecard = ManagerScorecard.Generera(
            maria.Id.Value, "2026-Q1",
            spanOfControl: 4,
            teamOmsattning: 5.2m,
            engagementDelta: 2.1m,
            utvecklingsplanFardiggrad: 68m,
            medelTidMellanOneonone: 12m);
        db.ManagerScorecards.Add(scorecard);

        // Coaching Nudges
        var nudge1 = CoachingNudge.Skapa(maria.Id.Value, "NewTeamMember", "Lars Nilsson började för 3 månader sedan — dags att boka uppföljande 1:1.");
        var nudge2 = CoachingNudge.Skapa(maria.Id.Value, "DevelopmentStalled", "Annas utvecklingsplan har inte uppdaterats på 45 dagar.");
        db.CoachingNudges.AddRange(nudge1, nudge2);

        // ============================================================
        // ONA — Organizational Network Analysis
        // ============================================================

        var onaSurvey = ONASurvey.Skapa("Samarbetsundersökning Q1 2026", "2026-Q1",
            """["Vem vänder du dig till för råd om arbetsuppgifter?","Vem samarbetar du mest med i vardagen?","Vem ger dig energi och inspiration?"]""");
        onaSurvey.Oppna();
        onaSurvey.Stang();
        db.ONASurveys.Add(onaSurvey);

        // ONA Responses (simulating survey answers between employees)
        var onaResponses = new[]
        {
            ONAResponse.Skapa(onaSurvey.Id, anna.Id.Value, maria.Id.Value, 0, 5),
            ONAResponse.Skapa(onaSurvey.Id, anna.Id.Value, erik.Id.Value, 1, 4),
            ONAResponse.Skapa(onaSurvey.Id, erik.Id.Value, anna.Id.Value, 0, 4),
            ONAResponse.Skapa(onaSurvey.Id, erik.Id.Value, maria.Id.Value, 1, 3),
            ONAResponse.Skapa(onaSurvey.Id, erik.Id.Value, karin.Id.Value, 2, 4),
            ONAResponse.Skapa(onaSurvey.Id, maria.Id.Value, anna.Id.Value, 0, 5),
            ONAResponse.Skapa(onaSurvey.Id, maria.Id.Value, karin.Id.Value, 1, 4),
            ONAResponse.Skapa(onaSurvey.Id, lars.Id.Value, anna.Id.Value, 0, 5),
            ONAResponse.Skapa(onaSurvey.Id, lars.Id.Value, erik.Id.Value, 1, 4),
            ONAResponse.Skapa(onaSurvey.Id, lars.Id.Value, maria.Id.Value, 2, 3),
            ONAResponse.Skapa(onaSurvey.Id, karin.Id.Value, maria.Id.Value, 0, 3),
            ONAResponse.Skapa(onaSurvey.Id, karin.Id.Value, anna.Id.Value, 2, 4),
        };
        db.ONAResponses.AddRange(onaResponses);

        // Run ONA calculation and persist
        var onaResult = ONACalculationService.Berakna(onaSurvey.Id, onaResponses);
        db.NetworkNodes.AddRange(onaResult.Nodes);
        db.NetworkEdges.AddRange(onaResult.Edges);
        onaSurvey.Analysera();

        db.SaveChanges();
    }
}
