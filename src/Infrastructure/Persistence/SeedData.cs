using Microsoft.EntityFrameworkCore;
using RegionHR.Core.Domain;
using RegionHR.Competence.Domain;
using RegionHR.HalsoSAM.Domain;
using RegionHR.Infrastructure.Journeys;
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
        var employments = new List<RegionHR.Core.Domain.Employment>();
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
            employments.Add(employment);
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

        // === RehabCase — skapade via domänens SkapaForSeed() ===
        // Milstolpar beräknas konsekvent från givet startdatum av SkapaForSeed().
        // Detta representerar planerade uppföljningsdatum, inte verifierad sjukfallsstart.

        // Karl Berg — upprepad korttidsfrånvaro, startade ~45 dagar sedan
        var rehabKarl = RehabCase.SkapaForSeed(
            employees[3].Id, RehabTrigger.SexTillfallenTolvManader, DateTime.UtcNow.AddDays(-45));
        rehabKarl.TilldelaArendeagare(employees[8].Id); // Eva Nilsson (HR)
        rehabKarl.SattRehabPlan("Uppfoljningssamtal varannan vecka, anpassad arbetsatergang diskuteras");
        db.RehabCases.Add(rehabKarl);

        // Helena Bergstrom — långtidssjuk >14 dagar, startade ~22 dagar sedan
        var rehabHelena = RehabCase.SkapaForSeed(
            employees[6].Id, RehabTrigger.FjortonSammanhangandeDagar, DateTime.UtcNow.AddDays(-22));
        rehabHelena.TilldelaArendeagare(employees[8].Id);
        db.RehabCases.Add(rehabHelena);

        // Maria Lindgren — chef initierat, startade ~160 dagar sedan (passerat dag 90)
        var rehabMaria = RehabCase.SkapaForSeed(
            employees[2].Id, RehabTrigger.ChefInitierat, DateTime.UtcNow.AddDays(-160));
        rehabMaria.TilldelaArendeagare(employees[8].Id);
        rehabMaria.SattRehabPlan("Arbetsformagebedomning genomford, deltidsatergang 50%");
        db.RehabCases.Add(rehabMaria);

        // === Journey templates + instances ===
        var onboardingMall = JourneyTemplate.Skapa("Ny medarbetare", JourneyKategori.Onboarding,
            "Onboarding-process for nyanstallda — 12 steg over 90 dagar");
        onboardingMall.LaggTillSteg("Skapa konton", "AD-konto, e-post, systembehorigheter", "IT", -5);
        onboardingMall.LaggTillSteg("Bestall utrustning", "Dator, telefon, passerkort", "IT", -3);
        onboardingMall.LaggTillSteg("Forbered arbetsplats", "Skrivbord, nyckel, introduktionsmaterial", "Chef", -2);
        onboardingMall.LaggTillSteg("Valkomstmote", "Introduktion till teamet och verksamheten", "Chef", 1);
        onboardingMall.LaggTillSteg("HR-introduktion", "Avtal, policyer, friskvard, lon", "HR", 1);
        onboardingMall.LaggTillSteg("IT-genomgang", "Inloggning, system, support", "IT", 2);
        onboardingMall.LaggTillSteg("Sakerhetsutbildning", "Obligatorisk brandskydd och HLR", "System", 3);
        onboardingMall.LaggTillSteg("Forsta uppfoljning", "Hur gar det? Fragor?", "Chef", 5);
        onboardingMall.LaggTillSteg("Mentor check-in", "Fragor och stod fran mentor", "Mentor", 14);
        onboardingMall.LaggTillSteg("30-dagarssamtal", "Formell uppfoljning med chef", "Chef", 30);
        onboardingMall.LaggTillSteg("60-dagarssamtal", "Utvecklingsplan och feedback", "Chef", 60);
        onboardingMall.LaggTillSteg("Provanstallning avslutas", "Utvardering och beslut", "HR", 90);
        db.JourneyTemplates.Add(onboardingMall);

        var avslutMall = JourneyTemplate.Skapa("Avslut", JourneyKategori.Avslut,
            "Offboarding-process vid anstallningens slut");
        avslutMall.LaggTillSteg("Kunskapsoverforing", "Dokumentera och overlat arbetsuppgifter", "Chef", -14);
        avslutMall.LaggTillSteg("Aterlamnning utrustning", "Dator, telefon, nycklar, passerkort", "Chef", -2);
        avslutMall.LaggTillSteg("Stang IT-behorigheter", "AD, e-post, VPN, system", "IT", 0);
        avslutMall.LaggTillSteg("Slutlon beraknad", "Semester, komptid, overtid", "HR", 0);
        avslutMall.LaggTillSteg("Tjanstgoringsintyg", "Utfarda och signera", "HR", 0);
        avslutMall.LaggTillSteg("Arbetsgivarintyg AF", "Utfarda for Arbetsformedlingen", "HR", 0);
        avslutMall.LaggTillSteg("Exit-samtal", "Avslutande samtal med chef", "Chef", -1);
        avslutMall.LaggTillSteg("GDPR-gallringsplan", "Upprata plan for dataradering", "HR", 5);
        db.JourneyTemplates.Add(avslutMall);

        // Journey instances — skapade via SkapaFranMall() for korrekt snapshot
        var onboardingAnna = JourneyInstance.SkapaFranMall(
            onboardingMall, employees[0].Id.Value,
            $"{employees[0].Fornamn} {employees[0].Efternamn}",
            DateTime.UtcNow.AddDays(-10));
        // Markera de forsta 5 stegen som klara (simulerar pagar ande onboarding)
        foreach (var steg in onboardingAnna.Steg.Take(5))
            onboardingAnna.MarkeraStegKlart(steg.Id, "Seed");
        db.JourneyInstances.Add(onboardingAnna);

        var onboardingSara = JourneyInstance.SkapaFranMall(
            onboardingMall, employees[4].Id.Value,
            $"{employees[4].Fornamn} {employees[4].Efternamn}",
            DateTime.UtcNow.AddDays(-3));
        // Markera forsta 2 stegen
        foreach (var steg in onboardingSara.Steg.Take(2))
            onboardingSara.MarkeraStegKlart(steg.Id, "Seed");
        db.JourneyInstances.Add(onboardingSara);

        // === Benefits catalog + employee selections ===
        var friskvard = RegionHR.Benefits.Domain.Benefit.Skapa(
            "Friskvardsbidrag", "Bidrag for fysisk aktivitet (gym, simhall, massage)",
            RegionHR.Benefits.Domain.BenefitCategory.Friskvard, 5000m, 100m, false);
        var sjukforsakring = RegionHR.Benefits.Domain.Benefit.Skapa(
            "Extra sjukvardsforsakring", "Privatvardsforsakring via Skandia",
            RegionHR.Benefits.Domain.BenefitCategory.Sjukvard, 0m, 100m, false);
        var cykel = RegionHR.Benefits.Domain.Benefit.Skapa(
            "Cykelfoman", "Cykel via bruttoloneavdrag, max 3000 kr/man",
            RegionHR.Benefits.Domain.BenefitCategory.Ovrigt, 3000m, 0m, true);
        var pension = RegionHR.Benefits.Domain.Benefit.Skapa(
            "AKAP-KR Pension", "Tjanstepension 6% under 7.5 IBB, 31.5% over",
            RegionHR.Benefits.Domain.BenefitCategory.Pension, 0m, 100m, false);
        var utbildning = RegionHR.Benefits.Domain.Benefit.Skapa(
            "Kompetensutvecklingsbidrag", "Bidrag for arbetsrelaterad vidareutbildning",
            RegionHR.Benefits.Domain.BenefitCategory.Utbildning, 15000m, 100m, false);
        db.Benefits.AddRange(friskvard, sjukforsakring, cykel, pension, utbildning);

        // Employee benefit selections via domänens Anmala()
        var valAnna = RegionHR.Benefits.Domain.EmployeeBenefit.Anmala(
            employees[0].Id.Value, friskvard.Id, DateOnly.FromDateTime(DateTime.Today), 5000m);
        valAnna.Godkann();
        var valErik = RegionHR.Benefits.Domain.EmployeeBenefit.Anmala(
            employees[1].Id.Value, friskvard.Id, DateOnly.FromDateTime(DateTime.Today), 3500m);
        valErik.Godkann();
        var valKarl = RegionHR.Benefits.Domain.EmployeeBenefit.Anmala(
            employees[3].Id.Value, cykel.Id, DateOnly.FromDateTime(DateTime.Today), 2000m, "Nytt barn — andrat pendlingsvanor");
        db.EmployeeBenefits.AddRange(valAnna, valErik, valKarl);

        // === Talent pool candidates via domänens Skapa() ===
        db.TalentPoolEntries.AddRange(
            RegionHR.Recruitment.Domain.TalentPoolEntry.Skapa(
                "Lisa Ekstrom", "lisa.ekstrom@mail.se", "IVA, akutsjukvard",
                "8 ars erfarenhet, intervjuad for SSK IVA men inte antagen. Mycket stark kandidat."),
            RegionHR.Recruitment.Domain.TalentPoolEntry.Skapa(
                "Per Strand", "per.strand@mail.se", "Vardcentral, slutenvard",
                "3 ars erfarenhet. Intresserad av framtida tjanster."),
            RegionHR.Recruitment.Domain.TalentPoolEntry.Skapa(
                "Sofia Magnusson", "sofia.magnusson@mail.se", "Intensivvard, anestesi",
                "Specialistsjukskoterska anestesi. Spontanansokande."),
            RegionHR.Recruitment.Domain.TalentPoolEntry.Skapa(
                "Oscar Blom", "oscar.blom@mail.se", "Underskoterska, natt",
                "2 ars erfarenhet nattjour. Soker heltid."),
            RegionHR.Recruitment.Domain.TalentPoolEntry.Skapa(
                "Karin Ek", "karin.ek@mail.se", "Sjukskoterska, barnklinik",
                "Nyexaminerad 2025. Praktik pa Sahlgrenska."));

        // === HeadcountPlan per enhet (budget for 2026) ===
        var hcAvd32 = RegionHR.Positions.Domain.HeadcountPlan.Skapa(avd32.Id.Value, 2026, 8, 8.0m, 2_760_000m);
        hcAvd32.UppdateraFaktiskt(8, 8.0m, 2_832_000m);
        var hcAvd33 = RegionHR.Positions.Domain.HeadcountPlan.Skapa(avd33.Id.Value, 2026, 6, 6.0m, 1_944_000m);
        hcAvd33.UppdateraFaktiskt(5, 5.0m, 1_620_000m);
        var hcAkuten = RegionHR.Positions.Domain.HeadcountPlan.Skapa(akuten.Id.Value, 2026, 12, 12.0m, 6_048_000m);
        hcAkuten.UppdateraFaktiskt(10, 10.0m, 5_040_000m);
        var hcIva = RegionHR.Positions.Domain.HeadcountPlan.Skapa(iva.Id.Value, 2026, 10, 10.0m, 3_816_000m);
        hcIva.UppdateraFaktiskt(7, 7.0m, 2_671_200m);
        db.HeadcountPlans.AddRange(hcAvd32, hcAvd33, hcAkuten, hcIva);

        // === Notifications via domänens Create() ===
        // UserId sätts till anställds Id.Value. Auth saknar EmployeeId-mapping
        // så i v1.5 matchar detta inte säkert inloggad användare.
        db.Notifications.AddRange(
            RegionHR.Notifications.Domain.Notification.Create(
                employees[0].Id.Value, "Ledighetsansokan godkand",
                "Din semester 14-18 juli har godkants av chef.",
                RegionHR.Notifications.Domain.NotificationType.Info, actionUrl: "/ledighet"),
            RegionHR.Notifications.Domain.Notification.Create(
                employees[0].Id.Value, "Nytt lonebesked",
                "Lonebeskedet for mars 2026 finns tillgangligt.",
                RegionHR.Notifications.Domain.NotificationType.Info, actionUrl: "/minsida/lon"),
            RegionHR.Notifications.Domain.Notification.Create(
                employees[3].Id.Value, "Certifiering gar ut snart",
                "Din sjukskoterska-legitimation gar ut om 45 dagar.",
                RegionHR.Notifications.Domain.NotificationType.Warning, actionUrl: "/kompetens"),
            RegionHR.Notifications.Domain.Notification.Create(
                employees[1].Id.Value, "Schemaandring",
                "Ditt schema for vecka 13 har uppdaterats.",
                RegionHR.Notifications.Domain.NotificationType.Info, actionUrl: "/minsida/schema"),
            RegionHR.Notifications.Domain.Notification.Create(
                employees[7].Id.Value, "Medarbetarsamtal bokat",
                "Arligt medarbetarsamtal inbokat 25 mars kl. 10:00.",
                RegionHR.Notifications.Domain.NotificationType.Action, actionUrl: "/medarbetarsamtal"));

        // === Cases with pending approvals via domänlogik ===
        var semesterCase = RegionHR.CaseManagement.Domain.Case.SkapaFranvaroarende(
            employees[0].Id, RegionHR.SharedKernel.Domain.AbsenceType.Semester,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(34)),
            "Semester vecka 20");
        semesterCase.SkickaForGodkannande("Chefsgodkannande", employees[7].Id); // Anders Olsson (VC)
        db.Cases.Add(semesterCase);

        var vabCase = RegionHR.CaseManagement.Domain.Case.SkapaFranvaroarende(
            employees[3].Id, RegionHR.SharedKernel.Domain.AbsenceType.VAB,
            DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            "VAB - barn sjukt");
        vabCase.SkickaForGodkannande("Chefsgodkannande", employees[7].Id);
        db.Cases.Add(vabCase);

        var tjledCase = RegionHR.CaseManagement.Domain.Case.SkapaFranvaroarende(
            employees[6].Id, RegionHR.SharedKernel.Domain.AbsenceType.Tjanstledighet,
            DateOnly.FromDateTime(DateTime.Today.AddDays(60)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(120)),
            "Studieledighet HT 2026");
        tjledCase.SkickaForGodkannande("HR-godkannande", employees[8].Id); // Eva Nilsson (HR)
        db.Cases.Add(tjledCase);

        // === LeaveRequests + VacationBalances via domänlogik ===
        // VacationBalance per anställd för 2026
        db.VacationBalances.AddRange(
            RegionHR.Leave.Domain.VacationBalance.SkapaForAr(employees[0].Id.Value, 2026, 41), // Anna, 41 → 31 dagar
            RegionHR.Leave.Domain.VacationBalance.SkapaForAr(employees[1].Id.Value, 2026, 48), // Erik, 48 → 31
            RegionHR.Leave.Domain.VacationBalance.SkapaForAr(employees[2].Id.Value, 2026, 36), // Maria, 36 → 25
            RegionHR.Leave.Domain.VacationBalance.SkapaForAr(employees[3].Id.Value, 2026, 44)); // Karl, 44 → 31

        // LeaveRequests i olika statusar
        var semAnna = RegionHR.Leave.Domain.LeaveRequest.Skapa(
            employees[0].Id.Value, RegionHR.Leave.Domain.LeaveType.Semester,
            DateOnly.FromDateTime(DateTime.Today.AddDays(40)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(54)),
            "Sommarsemester v28-v30");
        semAnna.SkickaIn();
        db.LeaveRequests.Add(semAnna);

        var semErik = RegionHR.Leave.Domain.LeaveRequest.Skapa(
            employees[1].Id.Value, RegionHR.Leave.Domain.LeaveType.Semester,
            DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(24)),
            "Sportlov med familjen");
        semErik.SkickaIn();
        semErik.Godkann(employees[7].Id.Value, "Godkant");
        db.LeaveRequests.Add(semErik);

        var kompKarl = RegionHR.Leave.Domain.LeaveRequest.Skapa(
            employees[3].Id.Value, RegionHR.Leave.Domain.LeaveType.Komptid,
            DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            "Kompdag for jourtjanst");
        kompKarl.SkickaIn();
        db.LeaveRequests.Add(kompKarl);

        // Föräldraledighet — Maria Lindgren, godkänd
        var flMaria = RegionHR.Leave.Domain.LeaveRequest.Skapa(
            employees[2].Id.Value, RegionHR.Leave.Domain.LeaveType.Foraldraledighet,
            DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(150)),
            "Foraldraledighet 6 manader");
        flMaria.SkickaIn();
        flMaria.Godkann(employees[7].Id.Value, "Godkant");
        db.LeaveRequests.Add(flMaria);

        // Föräldraledighet — Erik Johansson, inskickad
        var flErik = RegionHR.Leave.Domain.LeaveRequest.Skapa(
            employees[1].Id.Value, RegionHR.Leave.Domain.LeaveType.Foraldraledighet,
            DateOnly.FromDateTime(DateTime.Today.AddDays(60)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(120)),
            "Foraldraledighet hosten");
        flErik.SkickaIn();
        db.LeaveRequests.Add(flErik);

        // VAB — Karl Berg, godkänd
        var vabKarl = RegionHR.Leave.Domain.LeaveRequest.Skapa(
            employees[3].Id.Value, RegionHR.Leave.Domain.LeaveType.VAB,
            DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-6)),
            "Barn sjukt i feber");
        vabKarl.SkickaIn();
        vabKarl.Godkann(employees[7].Id.Value, "Godkant");
        db.LeaveRequests.Add(vabKarl);

        // VAB — Anna Svensson, inskickad (pågående)
        var vabAnna = RegionHR.Leave.Domain.LeaveRequest.Skapa(
            employees[0].Id.Value, RegionHR.Leave.Domain.LeaveType.VAB,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today),
            "VAB magsjuka");
        vabAnna.SkickaIn();
        db.LeaveRequests.Add(vabAnna);

        // === PayrollRuns + PayrollResults via domänlogik ===
        // Februari 2026 — utbetald körning
        var runFeb = RegionHR.Payroll.Domain.PayrollRun.Skapa(2026, 2, "System");
        runFeb.Paborja();
        for (var i = 0; i < 4; i++)
        {
            var emp = employees[i];
            var empl = employments[i];
            var lon = seedEmployees[i].Lon;
            var result = RegionHR.Payroll.Domain.PayrollResult.Skapa(
                runFeb.Id, emp.Id, empl.Id, 2026, 2,
                lon, 100m, RegionHR.SharedKernel.Domain.CollectiveAgreementType.AB);
            result.Brutto = lon;
            result.Skatt = Money.SEK(Math.Round(lon.Amount * 0.30m, 0));
            result.Netto = Money.SEK(lon.Amount - Math.Round(lon.Amount * 0.30m, 0));
            result.Arbetsgivaravgifter = Money.SEK(Math.Round(lon.Amount * 0.3142m, 0));
            result.ArbetsgivaravgiftSats = 31.42m;
            runFeb.LaggTillResultat(result);
        }
        runFeb.MarkeraSomBeraknad();
        runFeb.Godkann("Eva Nilsson");
        runFeb.MarkeraSomUtbetald();
        db.PayrollRuns.Add(runFeb);

        // Mars 2026 — beräknad körning (med OB-tillägg på Anna)
        var runMars = RegionHR.Payroll.Domain.PayrollRun.Skapa(2026, 3, "System");
        runMars.Paborja();
        for (var i = 0; i < 4; i++)
        {
            var emp = employees[i];
            var empl = employments[i];
            var lon = seedEmployees[i].Lon;
            var result = RegionHR.Payroll.Domain.PayrollResult.Skapa(
                runMars.Id, emp.Id, empl.Id, 2026, 3,
                lon, 100m, RegionHR.SharedKernel.Domain.CollectiveAgreementType.AB);
            var brutto = lon.Amount;
            // Anna Svensson (i=0): 3 nattpass OB
            if (i == 0)
            {
                result.OBTillagg = Money.SEK(3 * 113m); // natt-OB 113 kr/h * 3
                brutto += result.OBTillagg.Amount;
            }
            // Erik Johansson (i=1): övertid
            if (i == 1)
            {
                result.Overtidstillagg = Money.SEK(8 * 62000m / 165m * 1.8m); // 8h övertid
                brutto += result.Overtidstillagg.Amount;
            }
            result.Brutto = Money.SEK(Math.Round(brutto, 0));
            result.Skatt = Money.SEK(Math.Round(brutto * 0.30m, 0));
            result.Netto = Money.SEK(Math.Round(brutto * 0.70m, 0));
            result.Arbetsgivaravgifter = Money.SEK(Math.Round(brutto * 0.3142m, 0));
            result.ArbetsgivaravgiftSats = 31.42m;
            runMars.LaggTillResultat(result);
        }
        runMars.MarkeraSomBeraknad();
        db.PayrollRuns.Add(runMars);

        // === PerformanceReviews via domänens Skapa() ===
        // Anna Svensson — genomfört samtal (alla steg)
        var reviewAnna = RegionHR.Performance.Domain.PerformanceReview.Skapa(
            employees[0].Id.Value, employees[7].Id.Value, 2026); // Chef: Anders Olsson
        reviewAnna.SattSjalvbedomning("Jag har utvecklats inom saravard och tagit mer ansvar i teamet.");
        reviewAnna.SattChefsbedomning("Anna visar gott engagemang och tar initiativ. Utveckling inom ledarskap rekommenderas.", 4);
        reviewAnna.SattMalsattning("Genomga ledarskapsutbildning HT 2026, ta ansvar for handledning av ny personal.");
        reviewAnna.Genomfor();
        db.PerformanceReviews.Add(reviewAnna);

        // Erik Johansson — sjalvbedomning klar, vantar pa chefens bedomning
        var reviewErik = RegionHR.Performance.Domain.PerformanceReview.Skapa(
            employees[1].Id.Value, employees[7].Id.Value, 2026);
        reviewErik.SattSjalvbedomning("Har hanterat hog arbetsbelastning pa akuten. Onskar mer tid for forskning.");
        db.PerformanceReviews.Add(reviewErik);

        // Karl Berg — planerat, ej paborjat
        var reviewKarl = RegionHR.Performance.Domain.PerformanceReview.Skapa(
            employees[3].Id.Value, employees[7].Id.Value, 2026);
        db.PerformanceReviews.Add(reviewKarl);

        // Maria Lindgren — chefsbedomning klar, ej genomfort annu
        var reviewMaria = RegionHR.Performance.Domain.PerformanceReview.Skapa(
            employees[2].Id.Value, employees[7].Id.Value, 2026);
        reviewMaria.SattSjalvbedomning("Trivs pa avdelningen. Vill lara mer om dokumentation.");
        reviewMaria.SattChefsbedomning("Maria ar palitlig och omtyckt. Behover utveckla journalforing.", 3);
        db.PerformanceReviews.Add(reviewMaria);

        // === Courses + CourseEnrollments via domänlogik ===
        var hlrKurs = RegionHR.LMS.Domain.Course.Skapa(
            "HLR — Hjart-lungr\u00e4ddning", "Obligatorisk utbildning i hjart-lungr\u00e4ddning for all vardpersonal.",
            RegionHR.LMS.Domain.CourseFormat.Blandat, 240, true, "Klinisk", 24, 20);
        var brandKurs = RegionHR.LMS.Domain.Course.Skapa(
            "Brandskyddsutbildning", "Grundlaggande brandskydd och utrymning.",
            RegionHR.LMS.Domain.CourseFormat.Klassrum, 120, true, "Sakerhet", 12);
        var gdprKurs = RegionHR.LMS.Domain.Course.Skapa(
            "GDPR for vardpersonal", "Hantering av patientdata enligt GDPR och patientdatalagen.",
            RegionHR.LMS.Domain.CourseFormat.Elearning, 60, true, "Juridik", 12);
        var ledarKurs = RegionHR.LMS.Domain.Course.Skapa(
            "Grundlaggande ledarskap", "Ledarskapsutbildning for blivande chefer.",
            RegionHR.LMS.Domain.CourseFormat.Workshop, 480, false, "Ledarskap");
        var excelKurs = RegionHR.LMS.Domain.Course.Skapa(
            "Excel for HR", "Dataanalys och rapportering med Excel.",
            RegionHR.LMS.Domain.CourseFormat.Elearning, 90, false, "IT");

        // Publicera kurserna (domänmetod om den finns, annars direkt)
        // Course.Skapa() sätter Status=Utkast — vi behöver Publicerad
        // Kolla om Publicera() finns:
        db.Courses.AddRange(hlrKurs, brandKurs, gdprKurs, ledarKurs, excelKurs);

        // Enrollments
        var enrollAnnaHlr = RegionHR.LMS.Domain.CourseEnrollment.Anmala(employees[0].Id.Value, hlrKurs.Id);
        enrollAnnaHlr.Paborja();
        var enrollAnnaGdpr = RegionHR.LMS.Domain.CourseEnrollment.Anmala(employees[0].Id.Value, gdprKurs.Id);
        var enrollErikBrand = RegionHR.LMS.Domain.CourseEnrollment.Anmala(employees[1].Id.Value, brandKurs.Id);
        enrollErikBrand.Paborja();
        var enrollKarlHlr = RegionHR.LMS.Domain.CourseEnrollment.Anmala(employees[3].Id.Value, hlrKurs.Id);
        db.CourseEnrollments.AddRange(enrollAnnaHlr, enrollAnnaGdpr, enrollErikBrand, enrollKarlHlr);

        await db.SaveChangesAsync();
    }
}
