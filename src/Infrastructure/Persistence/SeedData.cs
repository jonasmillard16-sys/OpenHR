using Microsoft.EntityFrameworkCore;
using RegionHR.Core.Domain;
using RegionHR.Competence.Domain;
using RegionHR.Positions.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(RegionHRDbContext db)
    {
        if (await db.Employees.AnyAsync()) return; // Already seeded

        // === Organization units ===
        var region = OrganizationUnit.Skapa(
            "Region Vastra Gotaland", OrganizationUnitType.Region,
            "10000", DateOnly.FromDateTime(DateTime.Today.AddYears(-20)));

        var sjukhus = OrganizationUnit.Skapa(
            "Sahlgrenska Universitetssjukhuset", OrganizationUnitType.Forvaltning,
            "20000", DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            overordnadEnhetId: region.Id);

        var avd32 = OrganizationUnit.Skapa(
            "Avdelning 32", OrganizationUnitType.Avdelning,
            "20032", DateOnly.FromDateTime(DateTime.Today.AddYears(-10)),
            overordnadEnhetId: sjukhus.Id);

        var avd33 = OrganizationUnit.Skapa(
            "Avdelning 33", OrganizationUnitType.Avdelning,
            "20033", DateOnly.FromDateTime(DateTime.Today.AddYears(-10)),
            overordnadEnhetId: sjukhus.Id);

        var akuten = OrganizationUnit.Skapa(
            "Akutmottagningen", OrganizationUnitType.Avdelning,
            "20050", DateOnly.FromDateTime(DateTime.Today.AddYears(-15)),
            overordnadEnhetId: sjukhus.Id);

        var iva = OrganizationUnit.Skapa(
            "IVA", OrganizationUnitType.Avdelning,
            "20060", DateOnly.FromDateTime(DateTime.Today.AddYears(-15)),
            overordnadEnhetId: sjukhus.Id);

        db.OrganizationUnits.AddRange(region, sjukhus, avd32, avd33, akuten, iva);

        // === Employees ===
        var seedEmployees = new (string Fornamn, string Efternamn, string Pnr, string Befattning, OrganizationId Enhet, Money Lon)[]
        {
            ("Anna", "Svensson", "198503152383", "Sjukskoterska", avd32.Id, Money.SEK(34500m)),
            ("Erik", "Johansson", "197806221211", "Lakare", akuten.Id, Money.SEK(62000m)),
            ("Maria", "Lindgren", "199001015604", "Underskoterska", avd33.Id, Money.SEK(27800m)),
            ("Karl", "Berg", "198207143499", "Sjukskoterska", iva.Id, Money.SEK(35200m)),
            ("Sara", "Karlsson", "199504307843", "Underskoterska", avd32.Id, Money.SEK(27500m)),
            ("Johan", "Nilsson", "198802152382", "Lakare", akuten.Id, Money.SEK(58000m)),
            ("Helena", "Bergstrom", "199209184528", "Sjukskoterska", avd33.Id, Money.SEK(33800m)),
            ("Anders", "Olsson", "197503056789", "Verksamhetschef", sjukhus.Id, Money.SEK(52000m)),
            ("Eva", "Nilsson", "198007121303", "HR-chef", sjukhus.Id, Money.SEK(48000m)),
            ("Per", "Andersson", "198705232466", "Underskoterska", iva.Id, Money.SEK(28200m)),
        };

        var employees = new List<Employee>();
        foreach (var (fornamn, efternamn, pnr, befattning, enhetId, lon) in seedEmployees)
        {
            var employee = Employee.Skapa(Personnummer.CreateValidated(pnr), fornamn, efternamn);
            employee.UppdateraKontaktuppgifter(
                $"{fornamn.ToLower()}.{efternamn.ToLower()}@regionvg.se",
                $"070-{Random.Shared.Next(100, 999)} {Random.Shared.Next(10, 99)} {Random.Shared.Next(10, 99)}",
                null);
            var startdatum = DateOnly.FromDateTime(DateTime.Today.AddYears(-Random.Shared.Next(1, 15)));
            var employment = employee.LaggTillAnstallning(
                enhetId, EmploymentType.Tillsvidare, CollectiveAgreementType.AB,
                lon, Percentage.FullTime, startdatum);
            employment.SattBefattning(befattning);
            db.Employees.Add(employee);
            employees.Add(employee);
        }

        // === Skills (normaliserad katalog) ===
        var hlr = Skill.Skapa("HLR", SkillCategory.Klinisk, "Hjart-lungr\u00e4ddning");
        var triage = Skill.Skapa("Triage", SkillCategory.Klinisk, "Prioritering av patienter");
        var lakemedel = Skill.Skapa("Lakemedelshantering", SkillCategory.Klinisk, "Administration och kontroll av lakemedel");
        var journal = Skill.Skapa("Journalforing", SkillCategory.Klinisk, "Dokumentation i patientjournal");
        var saravard = Skill.Skapa("Saravard", SkillCategory.Klinisk, "Omlaggning och savard");
        var ventilator = Skill.Skapa("Ventilatorvard", SkillCategory.Klinisk, "Hantering av respirator/ventilator");
        var ledarskap = Skill.Skapa("Ledarskap", SkillCategory.Ledarskap, "Arbetsledning och teamledning");
        var projektledning = Skill.Skapa("Projektledning", SkillCategory.Ledarskap, "Planering och genomforande av projekt");
        var excel = Skill.Skapa("Excel/Statistik", SkillCategory.Teknisk, "Dataanalys och rapportering");
        var it = Skill.Skapa("IT-system", SkillCategory.Teknisk, "Hantering av verksamhetssystem");
        var kommunikation = Skill.Skapa("Kommunikation", SkillCategory.Administration, "Muntlig och skriftlig kommunikation");
        var arbetsratt = Skill.Skapa("Arbetsratt", SkillCategory.Administration, "Kunskap om LAS, MBL, AML");

        db.Skills.AddRange(hlr, triage, lakemedel, journal, saravard, ventilator,
            ledarskap, projektledning, excel, it, kommunikation, arbetsratt);

        // === Positions (kopplade till anstallda via InnehavareAnstallId) ===
        var posSsk32 = Position.Skapa(avd32.Id.Value, "Sjukskoterska Avd 32", 34500, 100);
        posSsk32.Tillsatt(employees[0].Id.Value); // Anna Svensson
        var posLakAkut = Position.Skapa(akuten.Id.Value, "Lakare Akutmottagningen", 62000, 100);
        posLakAkut.Tillsatt(employees[1].Id.Value); // Erik Johansson
        var posUsk33 = Position.Skapa(avd33.Id.Value, "Underskoterska Avd 33", 27800, 100);
        posUsk33.Tillsatt(employees[2].Id.Value); // Maria Lindgren
        var posSskIva = Position.Skapa(iva.Id.Value, "Sjukskoterska IVA", 35200, 100);
        posSskIva.Tillsatt(employees[3].Id.Value); // Karl Berg
        var posVc = Position.Skapa(sjukhus.Id.Value, "Verksamhetschef", 52000, 100);
        posVc.Tillsatt(employees[7].Id.Value); // Anders Olsson

        db.Positions_Table.AddRange(posSsk32, posLakAkut, posUsk33, posSskIva, posVc);

        // === PositionSkillRequirements (kravprofiler per position) ===
        // Sjukskoterska Avd 32: HLR 3, Lakemedel 4, Journal 3, Saravard 3
        db.PositionSkillRequirements.AddRange(
            PositionSkillRequirement.Skapa(posSsk32.Id, hlr.Id, 3),
            PositionSkillRequirement.Skapa(posSsk32.Id, lakemedel.Id, 4),
            PositionSkillRequirement.Skapa(posSsk32.Id, journal.Id, 3),
            PositionSkillRequirement.Skapa(posSsk32.Id, saravard.Id, 3));

        // Lakare Akutmottagningen: HLR 5, Triage 5, Lakemedel 5, Journal 4
        db.PositionSkillRequirements.AddRange(
            PositionSkillRequirement.Skapa(posLakAkut.Id, hlr.Id, 5),
            PositionSkillRequirement.Skapa(posLakAkut.Id, triage.Id, 5),
            PositionSkillRequirement.Skapa(posLakAkut.Id, lakemedel.Id, 5),
            PositionSkillRequirement.Skapa(posLakAkut.Id, journal.Id, 4));

        // Underskoterska Avd 33: HLR 2, Saravard 3, Journal 2
        db.PositionSkillRequirements.AddRange(
            PositionSkillRequirement.Skapa(posUsk33.Id, hlr.Id, 2),
            PositionSkillRequirement.Skapa(posUsk33.Id, saravard.Id, 3),
            PositionSkillRequirement.Skapa(posUsk33.Id, journal.Id, 2));

        // Sjukskoterska IVA: HLR 5, Ventilator 4, Lakemedel 5, Journal 4
        db.PositionSkillRequirements.AddRange(
            PositionSkillRequirement.Skapa(posSskIva.Id, hlr.Id, 5),
            PositionSkillRequirement.Skapa(posSskIva.Id, ventilator.Id, 4),
            PositionSkillRequirement.Skapa(posSskIva.Id, lakemedel.Id, 5),
            PositionSkillRequirement.Skapa(posSskIva.Id, journal.Id, 4));

        // Verksamhetschef: Ledarskap 5, Kommunikation 4, Arbetsratt 3, Excel 3
        db.PositionSkillRequirements.AddRange(
            PositionSkillRequirement.Skapa(posVc.Id, ledarskap.Id, 5),
            PositionSkillRequirement.Skapa(posVc.Id, kommunikation.Id, 4),
            PositionSkillRequirement.Skapa(posVc.Id, arbetsratt.Id, 3),
            PositionSkillRequirement.Skapa(posVc.Id, excel.Id, 3));

        // === EmployeeSkills (vad de anstallda faktiskt kan) ===
        // Anna Svensson (SSK Avd32): HLR 4, Lakemedel 3, Journal 4, Saravard 4
        db.EmployeeSkills.AddRange(
            EmployeeSkill.Skapa(employees[0].Id.Value, hlr.Id, 4),
            EmployeeSkill.Skapa(employees[0].Id.Value, lakemedel.Id, 3),
            EmployeeSkill.Skapa(employees[0].Id.Value, journal.Id, 4),
            EmployeeSkill.Skapa(employees[0].Id.Value, saravard.Id, 4));

        // Erik Johansson (Lakare Akut): HLR 5, Triage 4, Lakemedel 5, Journal 3
        db.EmployeeSkills.AddRange(
            EmployeeSkill.Skapa(employees[1].Id.Value, hlr.Id, 5),
            EmployeeSkill.Skapa(employees[1].Id.Value, triage.Id, 4),
            EmployeeSkill.Skapa(employees[1].Id.Value, lakemedel.Id, 5),
            EmployeeSkill.Skapa(employees[1].Id.Value, journal.Id, 3));

        // Maria Lindgren (USK Avd33): HLR 2, Saravard 2, Journal 2
        db.EmployeeSkills.AddRange(
            EmployeeSkill.Skapa(employees[2].Id.Value, hlr.Id, 2),
            EmployeeSkill.Skapa(employees[2].Id.Value, saravard.Id, 2),
            EmployeeSkill.Skapa(employees[2].Id.Value, journal.Id, 2));

        // Karl Berg (SSK IVA): HLR 5, Ventilator 3, Lakemedel 4, Journal 3
        db.EmployeeSkills.AddRange(
            EmployeeSkill.Skapa(employees[3].Id.Value, hlr.Id, 5),
            EmployeeSkill.Skapa(employees[3].Id.Value, ventilator.Id, 3),
            EmployeeSkill.Skapa(employees[3].Id.Value, lakemedel.Id, 4),
            EmployeeSkill.Skapa(employees[3].Id.Value, journal.Id, 3));

        // Anders Olsson (VC): Ledarskap 4, Kommunikation 4, Arbetsratt 2, Excel 3
        db.EmployeeSkills.AddRange(
            EmployeeSkill.Skapa(employees[7].Id.Value, ledarskap.Id, 4),
            EmployeeSkill.Skapa(employees[7].Id.Value, kommunikation.Id, 4),
            EmployeeSkill.Skapa(employees[7].Id.Value, arbetsratt.Id, 2),
            EmployeeSkill.Skapa(employees[7].Id.Value, excel.Id, 3));

        // === Provisioning rules (default konfiguration) ===
        db.ProvisioningRules.AddRange(
            Provisioning.ProvisioningRule.Skapa(
                Provisioning.ProvisioningTrigger.NyAnstallning,
                "Active Directory",
                Provisioning.ProvisioningAktion.SkapaKonto,
                "Skapa AD-konto vid nyanstallning"),
            Provisioning.ProvisioningRule.Skapa(
                Provisioning.ProvisioningTrigger.NyAnstallning,
                "E-post (Exchange)",
                Provisioning.ProvisioningAktion.SkapaEpost,
                "Skapa e-postlada vid nyanstallning"),
            Provisioning.ProvisioningRule.Skapa(
                Provisioning.ProvisioningTrigger.AvslutadAnstallning,
                "Active Directory",
                Provisioning.ProvisioningAktion.InaktiveraKonto,
                "Inaktivera AD-konto vid avslut"),
            Provisioning.ProvisioningRule.Skapa(
                Provisioning.ProvisioningTrigger.AvslutadAnstallning,
                "Passersystem",
                Provisioning.ProvisioningAktion.SparraPasserkort,
                "Sparra passerkort vid avslut"));

        // === Arbetsmiljo: incidents, safety rounds, risk assessments ===
        db.Incidents.AddRange(
            Arbetsmiljo.Incident.Skapa(DateTime.UtcNow.AddDays(-12), "Anna Svensson", avd32.Id.Value,
                "Korridoren vid rum 4", "Halkolycka pa vatt golv efter stadning", Arbetsmiljo.IncidentAllvarlighetsgrad.Medel,
                Arbetsmiljo.IncidentTyp.Tillbud, "Battre skyltning vid vatgolv"),
            Arbetsmiljo.Incident.Skapa(DateTime.UtcNow.AddDays(-8), "Karl Berg", iva.Id.Value,
                "IVA sal 2", "Nalsticka vid blodprovstagning", Arbetsmiljo.IncidentAllvarlighetsgrad.Hog,
                Arbetsmiljo.IncidentTyp.Arbetsskada, "Genomgang av stickskadeprevention"),
            Arbetsmiljo.Incident.Skapa(DateTime.UtcNow.AddDays(-5), "Maria Lindgren", avd33.Id.Value,
                "Personalrum", "Ergonomisk brist — for laga arbetsstolar", Arbetsmiljo.IncidentAllvarlighetsgrad.Lag,
                Arbetsmiljo.IncidentTyp.Tillbud, "Bestall nya stolar"),
            Arbetsmiljo.Incident.Skapa(DateTime.UtcNow.AddDays(-3), "Erik Johansson", akuten.Id.Value,
                "Akutens vantrum", "Hotfull patient mot personal", Arbetsmiljo.IncidentAllvarlighetsgrad.Kritisk,
                Arbetsmiljo.IncidentTyp.Tillbud, "Genomgang av hot- och valdpolicy"),
            Arbetsmiljo.Incident.Skapa(DateTime.UtcNow.AddDays(-1), "Helena Bergstrom", avd33.Id.Value,
                "Medicinforraadet", "Felmarkerad medicinburk", Arbetsmiljo.IncidentAllvarlighetsgrad.Hog,
                Arbetsmiljo.IncidentTyp.Tillbud, "Kontroll av alla medicinforrrad"));

        db.SafetyRounds.AddRange(
            Arbetsmiljo.SafetyRound.Skapa(DateTime.UtcNow.AddDays(-30), avd32.Id.Value,
                "Anna Svensson, Skyddsombud Lars Ek", 3, Arbetsmiljo.SafetyRoundStatus.Genomford,
                "Brister: belysning korridor, handtag toalett, ventilation rum 6"),
            Arbetsmiljo.SafetyRound.Skapa(DateTime.UtcNow.AddDays(-15), iva.Id.Value,
                "Karl Berg, Skyddsombud Eva Lind", 1, Arbetsmiljo.SafetyRoundStatus.Genomford,
                "Brist: nododusch saknar skylt"),
            Arbetsmiljo.SafetyRound.Skapa(DateTime.UtcNow.AddDays(14), akuten.Id.Value,
                "Erik Johansson, Skyddsombud Maria Ek", 0, Arbetsmiljo.SafetyRoundStatus.Planerad, null),
            Arbetsmiljo.SafetyRound.Skapa(DateTime.UtcNow.AddDays(30), avd33.Id.Value,
                "Helena Bergstrom, Skyddsombud Per Lund", 0, Arbetsmiljo.SafetyRoundStatus.Planerad, null));

        db.RiskAssessments.AddRange(
            Arbetsmiljo.RiskAssessment.Skapa("Halkolyckor", "Risk for halkolyckor pa nystadade golv", 3, 2,
                "Installera anti-halkskyltning", "Facilitetsansvarig", DateOnly.FromDateTime(DateTime.Today.AddDays(30))),
            Arbetsmiljo.RiskAssessment.Skapa("Stickskador", "Risk for nalsticka vid blodprov och injektioner", 2, 4,
                "Obligatorisk utbildning i sakra injektionstekniker", "Vardchef", DateOnly.FromDateTime(DateTime.Today.AddDays(60))),
            Arbetsmiljo.RiskAssessment.Skapa("Hot och vald", "Risk for hotfulla patienter pa akuten", 3, 5,
                "Utbildning i bemotande, overfallslarm, kamerapolicy", "Verksamhetschef", DateOnly.FromDateTime(DateTime.Today.AddDays(14))),
            Arbetsmiljo.RiskAssessment.Skapa("Ergonomi", "Risk for belastningsskador vid tunga lyft", 4, 3,
                "Lyfthjalpmedel, ergonomiutbildning", "Arbetsmiljosamordnare", DateOnly.FromDateTime(DateTime.Today.AddDays(45))),
            Arbetsmiljo.RiskAssessment.Skapa("Medicinhantering", "Risk for felmedicinering vid markning", 2, 5,
                "Dubbelsignering, barkodsystem", "Lakemedelsansvarig", DateOnly.FromDateTime(DateTime.Today.AddDays(90))));

        await db.SaveChangesAsync();
    }
}
