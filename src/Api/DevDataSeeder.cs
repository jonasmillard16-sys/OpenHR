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

        db.SaveChanges();
    }
}
