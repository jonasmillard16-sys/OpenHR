using Microsoft.EntityFrameworkCore;
using RegionHR.Core.Domain;
using RegionHR.Agreements.Domain;
using RegionHR.Analytics.Domain;
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

        // === Collective Agreements (10 avtal) ===
        var giltigFran = new DateOnly(2025, 4, 1);

        // 1. AB — Allmanna bestammelser (kommun/region)
        var ab = CollectiveAgreement.Skapa("AB 2025", "SKR och Kommunal", giltigFran, IndustrySector.KommunRegion);
        ab.LaggTillOBSats(OBCategory.VardagKvall, 126.50m, giltigFran);
        ab.LaggTillOBSats(OBCategory.VardagNatt, 152.00m, giltigFran);
        ab.LaggTillOBSats(OBCategory.Helg, 89.00m, giltigFran);
        ab.LaggTillOBSats(OBCategory.Storhelg, 195.00m, giltigFran);
        ab.LaggTillOvertidsRegel(1.0m, 1.8m, 200m);
        ab.LaggTillSemesterRegel(25, 31, 32);
        ab.LaggTillPensionsRegel(PensionType.AKAPKR, 6.0m, 31.5m, 599500m, "{\"IBB\":599500,\"TakMultipel\":7.5}");
        ab.LaggTillViloRegel(11m, 36m, 0.5m);
        ab.LaggTillArbetstidsRegel(38.25m, "{\"MaxFlexSaldo\":40}");
        ab.LaggTillUppságningsRegel(0, 1);
        ab.LaggTillUppságningsRegel(24, 2);
        ab.LaggTillUppságningsRegel(72, 3);
        ab.LaggTillForsakringspaket("{\"Belopp\":285000}", "{\"DagErsattning\":810}", "{\"Omfattning\":\"Full\"}", "{\"Typ\":\"AFA\"}", "{\"Omfattning\":\"Standard\"}");
        ab.LaggTillLonestruktur("{\"Underskoterska\":25500,\"Sjukskoterska\":29000,\"Lakare\":42000}", "[]");

        // 2. HOK — Huvudoverenskommelse
        var hok = CollectiveAgreement.Skapa("HOK 2025", "SKR och OFR/S,P,O", giltigFran, IndustrySector.KommunRegion);
        hok.LaggTillOBSats(OBCategory.VardagKvall, 120.00m, giltigFran);
        hok.LaggTillOBSats(OBCategory.VardagNatt, 145.00m, giltigFran);
        hok.LaggTillOBSats(OBCategory.Helg, 85.00m, giltigFran);
        hok.LaggTillOBSats(OBCategory.Storhelg, 185.00m, giltigFran);
        hok.LaggTillOvertidsRegel(1.0m, 1.7m, 200m);
        hok.LaggTillSemesterRegel(25, 31, 32);
        hok.LaggTillPensionsRegel(PensionType.AKAPKR, 6.0m, 31.5m, 599500m, "{\"IBB\":599500,\"TakMultipel\":7.5}");
        hok.LaggTillViloRegel(11m, 36m, 0.5m);
        hok.LaggTillArbetstidsRegel(38.25m, "{\"MaxFlexSaldo\":40}");

        // 3. Teknikavtalet
        var teknik = CollectiveAgreement.Skapa("Teknikavtalet 2025", "Teknikarbetsgivarna och IF Metall", giltigFran, IndustrySector.IndustriTeknik);
        teknik.LaggTillOBSats(OBCategory.VardagKvall, 58.70m, giltigFran);
        teknik.LaggTillOBSats(OBCategory.VardagNatt, 78.50m, giltigFran);
        teknik.LaggTillOBSats(OBCategory.Helg, 44.00m, giltigFran);
        teknik.LaggTillOBSats(OBCategory.Storhelg, 105.00m, giltigFran);
        teknik.LaggTillOvertidsRegel(1.0m, 1.5m, 200m);
        teknik.LaggTillSemesterRegel(25, 27, 28);
        teknik.LaggTillPensionsRegel(PensionType.SAFLO, 4.5m, 30.0m, 599500m, "{\"IBB\":599500,\"TakMultipel\":7.5}");
        teknik.LaggTillViloRegel(11m, 36m, 0.5m);
        teknik.LaggTillArbetstidsRegel(40.0m, "{}");

        // 4. Handelsavtalet
        var handel = CollectiveAgreement.Skapa("Handelsavtalet 2025", "Svensk Handel och Handels", giltigFran, IndustrySector.Handel);
        handel.LaggTillOBSats(OBCategory.VardagKvall, 42.00m, giltigFran);
        handel.LaggTillOBSats(OBCategory.VardagNatt, 72.00m, giltigFran);
        handel.LaggTillOBSats(OBCategory.Helg, 55.00m, giltigFran);
        handel.LaggTillOBSats(OBCategory.Storhelg, 115.00m, giltigFran);
        handel.LaggTillOvertidsRegel(1.0m, 1.5m, 150m);
        handel.LaggTillSemesterRegel(25, 27, 28);
        handel.LaggTillPensionsRegel(PensionType.SAFLO, 4.5m, 30.0m, 599500m, "{\"IBB\":599500}");
        handel.LaggTillViloRegel(11m, 36m, 0.5m);
        handel.LaggTillArbetstidsRegel(40.0m, "{}");

        // 5. IT/Telekomavtalet
        var itTelekom = CollectiveAgreement.Skapa("IT/Telekomavtalet 2025", "Almega IT och Unionen", giltigFran, IndustrySector.ITTelekom);
        itTelekom.LaggTillOBSats(OBCategory.VardagKvall, 35.00m, giltigFran);
        itTelekom.LaggTillOBSats(OBCategory.VardagNatt, 65.00m, giltigFran);
        itTelekom.LaggTillOBSats(OBCategory.Helg, 45.00m, giltigFran);
        itTelekom.LaggTillOBSats(OBCategory.Storhelg, 95.00m, giltigFran);
        itTelekom.LaggTillOvertidsRegel(1.0m, 1.5m, 200m);
        itTelekom.LaggTillSemesterRegel(25, 28, 30);
        itTelekom.LaggTillPensionsRegel(PensionType.ITP1, 4.5m, 30.0m, 599500m, "{\"IBB\":599500}");
        itTelekom.LaggTillViloRegel(11m, 36m, 0.5m);
        itTelekom.LaggTillArbetstidsRegel(40.0m, "{\"MaxFlexSaldo\":80,\"KarnTidStart\":\"10:00\",\"KarnTidSlut\":\"15:00\"}");
        itTelekom.LaggTillPrivatErsattningsPlan("{\"MaxProcent\":15}", "{}", "{\"ESOP\":true}", "{\"MaxBelopp\":8000}");

        // 6. Vardforetagaravtalet
        var vardforetag = CollectiveAgreement.Skapa("Vardforetagaravtalet 2025", "Vardforetagarna och Kommunal", giltigFran, IndustrySector.SjukvardPrivat);
        vardforetag.LaggTillOBSats(OBCategory.VardagKvall, 110.00m, giltigFran);
        vardforetag.LaggTillOBSats(OBCategory.VardagNatt, 135.00m, giltigFran);
        vardforetag.LaggTillOBSats(OBCategory.Helg, 80.00m, giltigFran);
        vardforetag.LaggTillOBSats(OBCategory.Storhelg, 170.00m, giltigFran);
        vardforetag.LaggTillOvertidsRegel(1.0m, 1.7m, 200m);
        vardforetag.LaggTillSemesterRegel(25, 30, 31);
        vardforetag.LaggTillPensionsRegel(PensionType.SAFLO, 4.5m, 30.0m, 599500m, "{\"IBB\":599500}");
        vardforetag.LaggTillViloRegel(11m, 36m, 0.5m);
        vardforetag.LaggTillArbetstidsRegel(38.25m, "{}");

        // 7. Transportavtalet
        var transport = CollectiveAgreement.Skapa("Transportavtalet 2025", "Biltrafikens Arbetsgivareforb. och Transport", giltigFran, IndustrySector.Transport);
        transport.LaggTillOBSats(OBCategory.VardagKvall, 48.00m, giltigFran);
        transport.LaggTillOBSats(OBCategory.VardagNatt, 85.00m, giltigFran);
        transport.LaggTillOBSats(OBCategory.Helg, 60.00m, giltigFran);
        transport.LaggTillOBSats(OBCategory.Storhelg, 125.00m, giltigFran);
        transport.LaggTillOvertidsRegel(1.0m, 1.5m, 200m);
        transport.LaggTillSemesterRegel(25, 27, 28);
        transport.LaggTillPensionsRegel(PensionType.SAFLO, 4.5m, 30.0m, 599500m, "{\"IBB\":599500}");
        transport.LaggTillViloRegel(11m, 36m, 0.75m);
        transport.LaggTillArbetstidsRegel(40.0m, "{}");

        // 8. HRF-avtalet (Hotell och restaurang)
        var hrf = CollectiveAgreement.Skapa("HRF-avtalet 2025", "Visita och HRF", giltigFran, IndustrySector.HotellRestaurang);
        hrf.LaggTillOBSats(OBCategory.VardagKvall, 38.50m, giltigFran);
        hrf.LaggTillOBSats(OBCategory.VardagNatt, 58.00m, giltigFran);
        hrf.LaggTillOBSats(OBCategory.Helg, 48.00m, giltigFran);
        hrf.LaggTillOBSats(OBCategory.Storhelg, 98.00m, giltigFran);
        hrf.LaggTillOvertidsRegel(1.0m, 1.5m, 150m);
        hrf.LaggTillSemesterRegel(25, 26, 27);
        hrf.LaggTillPensionsRegel(PensionType.SAFLO, 4.5m, 30.0m, 599500m, "{\"IBB\":599500}");
        hrf.LaggTillViloRegel(11m, 36m, 0.5m);
        hrf.LaggTillArbetstidsRegel(40.0m, "{}");

        // 9. Tjanstemannaavtalet
        var tjansteman = CollectiveAgreement.Skapa("Tjanstemannaavtalet 2025", "Almega och Unionen", giltigFran, IndustrySector.Tjanstemannaallman);
        tjansteman.LaggTillOBSats(OBCategory.VardagKvall, 32.00m, giltigFran);
        tjansteman.LaggTillOBSats(OBCategory.VardagNatt, 55.00m, giltigFran);
        tjansteman.LaggTillOBSats(OBCategory.Helg, 42.00m, giltigFran);
        tjansteman.LaggTillOBSats(OBCategory.Storhelg, 88.00m, giltigFran);
        tjansteman.LaggTillOvertidsRegel(1.0m, 1.5m, 200m);
        tjansteman.LaggTillSemesterRegel(25, 28, 30);
        tjansteman.LaggTillPensionsRegel(PensionType.ITP1, 4.5m, 30.0m, 599500m, "{\"IBB\":599500}");
        tjansteman.LaggTillViloRegel(11m, 36m, 0.5m);
        tjansteman.LaggTillArbetstidsRegel(40.0m, "{\"MaxFlexSaldo\":40}");

        // 10. Avtalslost — tomt avtal for privat utan kollektivavtal
        var avtalslost = CollectiveAgreement.Skapa("Avtalslost", "Ingen", giltigFran, IndustrySector.Avtalslost);

        db.CollectiveAgreements.AddRange(ab, hok, teknik, handel, itTelekom, vardforetag, transport, hrf, tjansteman, avtalslost);

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
            employment.SattKollektivavtal(ab.Id);
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

        // Vakant position
        var posUskAkut = Position.Skapa(akuten.Id.Value, "Underskoterska Akutmottagningen", 27500, 75);
        // Frusen position
        var posItSjukhus = Position.Skapa(sjukhus.Id.Value, "IT-tekniker", 38000, 100);
        posItSjukhus.Frys();

        db.Positions_Table.AddRange(posSsk32, posLakAkut, posUsk33, posSskIva, posVc, posUskAkut, posItSjukhus);

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

        // === Skill Categories (entity-baserade, ersätter enum) ===
        var katKlinisk = SkillCategoryEntity.Skapa("Klinisk", "Kliniska vårdkompetenser");
        var katTeknisk = SkillCategoryEntity.Skapa("Teknisk", "Tekniska och IT-kompetenser");
        var katLedarskap = SkillCategoryEntity.Skapa("Ledarskap", "Ledarskaps- och chefskompetenser");
        var katAdmin = SkillCategoryEntity.Skapa("Administration", "Administrativa kompetenser");
        var katKomm = SkillCategoryEntity.Skapa("Kommunikation", "Kommunikations- och samverkanskompetenser");
        var katReg = SkillCategoryEntity.Skapa("Regulatorisk", "Regelverks- och lagstiftningskompetenser");

        db.SkillCategories.AddRange(katKlinisk, katTeknisk, katLedarskap, katAdmin, katKomm, katReg);

        // Koppla befintliga skills till nya kategori-entiteter
        hlr.SattKategoriEntitet(katKlinisk.Id);
        triage.SattKategoriEntitet(katKlinisk.Id);
        lakemedel.SattKategoriEntitet(katKlinisk.Id);
        journal.SattKategoriEntitet(katKlinisk.Id);
        saravard.SattKategoriEntitet(katKlinisk.Id);
        ventilator.SattKategoriEntitet(katKlinisk.Id);
        ledarskap.SattKategoriEntitet(katLedarskap.Id);
        projektledning.SattKategoriEntitet(katLedarskap.Id);
        excel.SattKategoriEntitet(katTeknisk.Id);
        it.SattKategoriEntitet(katTeknisk.Id);
        kommunikation.SattKategoriEntitet(katAdmin.Id);
        arbetsratt.SattKategoriEntitet(katAdmin.Id);

        // === Skill Relations ===
        db.SkillRelations.AddRange(
            SkillRelation.Skapa(hlr.Id, triage.Id, "Related"),
            SkillRelation.Skapa(lakemedel.Id, journal.Id, "Related"),
            SkillRelation.Skapa(ledarskap.Id, projektledning.Id, "Prerequisite"));

        // === Inferred Skills ===
        db.InferredSkills.AddRange(
            InferredSkill.Skapa(employees[0].Id.Value, triage.Id, "Befattning", 75),
            InferredSkill.Skapa(employees[1].Id.Value, ventilator.Id, "Erfarenhet", 60),
            InferredSkill.Skapa(employees[3].Id.Value, ledarskap.Id, "Kurs", 50));

        // === Career Paths ===
        var cpVard = CareerPath.Skapa("Sjukskoterska till Vardenhetschef", "Vard",
            "Karriarvag fran grundutbildad sjukskoterska till enhetschef");
        cpVard.LaggTillSteg("Sjukskoterska", 1, 24,
            "[{\"Skill\":\"HLR\",\"Niva\":3},{\"Skill\":\"Lakemedel\",\"Niva\":3}]", 0);
        cpVard.LaggTillSteg("Specialistsjukskoterska", 2, 36,
            "[{\"Skill\":\"HLR\",\"Niva\":5},{\"Skill\":\"Ventilatorvard\",\"Niva\":3}]", 24);
        cpVard.LaggTillSteg("Vardenhetschef", 3, 0,
            "[{\"Skill\":\"Ledarskap\",\"Niva\":4},{\"Skill\":\"Kommunikation\",\"Niva\":4}]", 60);

        var cpIt = CareerPath.Skapa("Utvecklare till Tech Lead", "IT",
            "Karriarvag inom IT-utveckling");
        cpIt.LaggTillSteg("Utvecklare", 1, 24,
            "[{\"Skill\":\"IT-system\",\"Niva\":3}]", 0);
        cpIt.LaggTillSteg("Senior Utvecklare", 2, 36,
            "[{\"Skill\":\"IT-system\",\"Niva\":4},{\"Skill\":\"Projektledning\",\"Niva\":2}]", 24);
        cpIt.LaggTillSteg("Tech Lead", 3, 0,
            "[{\"Skill\":\"IT-system\",\"Niva\":5},{\"Skill\":\"Ledarskap\",\"Niva\":3}]", 60);

        db.CareerPaths.AddRange(cpVard, cpIt);

        // === Internal Opportunity (published project) ===
        var opportunity = InternalOpportunity.Skapa("Projekt", "Digitalisering av patientjournaler",
            sjukhus.Id.Value,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
            "[{\"Skill\":\"IT-system\",\"Niva\":3},{\"Skill\":\"Journalforing\",\"Niva\":2}]");
        opportunity.Publicera();
        db.InternalOpportunities.Add(opportunity);

        // === Mentor Relations ===
        db.MentorRelations.AddRange(
            MentorRelation.Skapa(employees[7].Id.Value, employees[0].Id.Value, "Ledarskap och karriarutveckling",
                DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)), 14),
            MentorRelation.Skapa(employees[1].Id.Value, employees[3].Id.Value, "Klinisk specialisering",
                DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)), 7));

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

        // === AuditEntries via domänens Create() ===
        db.AuditEntries.AddRange(
            RegionHR.Audit.Domain.AuditEntry.Create(
                "Employee", employees[0].Id.Value.ToString(), RegionHR.Audit.Domain.AuditAction.Create,
                null, "{\"Fornamn\":\"Anna\",\"Efternamn\":\"Svensson\"}", "System", "System", null),
            RegionHR.Audit.Domain.AuditEntry.Create(
                "Employee", employees[0].Id.Value.ToString(), RegionHR.Audit.Domain.AuditAction.Update,
                "{\"Befattning\":\"Underskoterska\"}", "{\"Befattning\":\"Sjukskoterska\"}", employees[8].Id.Value.ToString(), "Eva Nilsson (HR)", null),
            RegionHR.Audit.Domain.AuditEntry.Create(
                "PayrollRun", Guid.NewGuid().ToString(), RegionHR.Audit.Domain.AuditAction.Create,
                null, "{\"Period\":\"2026-02\",\"Status\":\"Beraknad\"}", "System", "System", null),
            RegionHR.Audit.Domain.AuditEntry.Create(
                "LeaveRequest", Guid.NewGuid().ToString(), RegionHR.Audit.Domain.AuditAction.Create,
                null, "{\"Typ\":\"Semester\",\"FranDatum\":\"2026-07\"}", employees[0].Id.Value.ToString(), "Anna Svensson", null),
            RegionHR.Audit.Domain.AuditEntry.Create(
                "Position", posSsk32.Id.ToString(), RegionHR.Audit.Domain.AuditAction.Update,
                "{\"Status\":\"Vakant\"}", "{\"Status\":\"Aktiv\"}", employees[8].Id.Value.ToString(), "Eva Nilsson (HR)", null));

        // === DataSubjectRequests via domänens Skapa() ===
        var dsrRegisterutdrag = RegionHR.GDPR.Domain.DataSubjectRequest.Skapa(employees[4].Id.Value, RegionHR.GDPR.Domain.RequestType.Registerutdrag);
        dsrRegisterutdrag.Tilldela(employees[8].Id.Value.ToString()); // Eva Nilsson (HR)
        var dsrRadering = RegionHR.GDPR.Domain.DataSubjectRequest.Skapa(employees[5].Id.Value, RegionHR.GDPR.Domain.RequestType.Radering);
        db.DataSubjectRequests.AddRange(dsrRegisterutdrag, dsrRadering);

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

        // === ScheduledShifts (idag, baserat på DateTime.Today) ===
        var idagDatum = DateOnly.FromDateTime(DateTime.Today);
        var schemaId = ScheduleId.New();
        db.ScheduledShifts.AddRange(
            new RegionHR.Scheduling.Domain.ScheduledShift { Id = Guid.NewGuid(), SchemaId = schemaId, AnstallId = employees[0].Id, Datum = idagDatum, PassTyp = RegionHR.Scheduling.Domain.ShiftType.Dag, PlaneradStart = new TimeOnly(7, 0), PlaneradSlut = new TimeOnly(16, 0), Rast = TimeSpan.FromMinutes(60), Status = RegionHR.Scheduling.Domain.ShiftStatus.Planerad, OBKategori = OBCategory.Ingen },
            new RegionHR.Scheduling.Domain.ScheduledShift { Id = Guid.NewGuid(), SchemaId = schemaId, AnstallId = employees[2].Id, Datum = idagDatum, PassTyp = RegionHR.Scheduling.Domain.ShiftType.Dag, PlaneradStart = new TimeOnly(7, 0), PlaneradSlut = new TimeOnly(16, 0), Rast = TimeSpan.FromMinutes(60), Status = RegionHR.Scheduling.Domain.ShiftStatus.Planerad, OBKategori = OBCategory.Ingen },
            new RegionHR.Scheduling.Domain.ScheduledShift { Id = Guid.NewGuid(), SchemaId = schemaId, AnstallId = employees[4].Id, Datum = idagDatum, PassTyp = RegionHR.Scheduling.Domain.ShiftType.Dag, PlaneradStart = new TimeOnly(7, 0), PlaneradSlut = new TimeOnly(16, 0), Rast = TimeSpan.FromMinutes(60), Status = RegionHR.Scheduling.Domain.ShiftStatus.Planerad, OBKategori = OBCategory.Ingen },
            new RegionHR.Scheduling.Domain.ScheduledShift { Id = Guid.NewGuid(), SchemaId = schemaId, AnstallId = employees[6].Id, Datum = idagDatum, PassTyp = RegionHR.Scheduling.Domain.ShiftType.Kvall, PlaneradStart = new TimeOnly(15, 0), PlaneradSlut = new TimeOnly(22, 0), Rast = TimeSpan.FromMinutes(30), Status = RegionHR.Scheduling.Domain.ShiftStatus.Planerad, OBKategori = OBCategory.VardagKvall },
            new RegionHR.Scheduling.Domain.ScheduledShift { Id = Guid.NewGuid(), SchemaId = schemaId, AnstallId = employees[9].Id, Datum = idagDatum, PassTyp = RegionHR.Scheduling.Domain.ShiftType.Kvall, PlaneradStart = new TimeOnly(15, 0), PlaneradSlut = new TimeOnly(22, 0), Rast = TimeSpan.FromMinutes(30), Status = RegionHR.Scheduling.Domain.ShiftStatus.Planerad, OBKategori = OBCategory.VardagKvall },
            new RegionHR.Scheduling.Domain.ScheduledShift { Id = Guid.NewGuid(), SchemaId = schemaId, AnstallId = employees[3].Id, Datum = idagDatum, PassTyp = RegionHR.Scheduling.Domain.ShiftType.Natt, PlaneradStart = new TimeOnly(21, 0), PlaneradSlut = new TimeOnly(7, 0), Rast = TimeSpan.FromMinutes(45), Status = RegionHR.Scheduling.Domain.ShiftStatus.Planerad, OBKategori = OBCategory.VardagNatt });

        // === Certifications via domänens Skapa() ===
        db.Certifications.AddRange(
            RegionHR.Competence.Domain.Certification.Skapa(
                employees[0].Id.Value, "HLR", RegionHR.Competence.Domain.CertificationType.ObligatoriskUtbildning,
                "Svensk HLR-rad", DateOnly.FromDateTime(DateTime.Today.AddMonths(-6)),
                DateOnly.FromDateTime(DateTime.Today.AddMonths(6)), obligatorisk: true),
            RegionHR.Competence.Domain.Certification.Skapa(
                employees[0].Id.Value, "Brandskydd", RegionHR.Competence.Domain.CertificationType.ObligatoriskUtbildning,
                "MSB", DateOnly.FromDateTime(DateTime.Today.AddMonths(-4)),
                DateOnly.FromDateTime(DateTime.Today.AddMonths(8)), obligatorisk: true),
            RegionHR.Competence.Domain.Certification.Skapa(
                employees[0].Id.Value, "Lakemedelshantering", RegionHR.Competence.Domain.CertificationType.Legitimation,
                "Socialstyrelsen", DateOnly.FromDateTime(DateTime.Today.AddMonths(-18)),
                DateOnly.FromDateTime(DateTime.Today.AddMonths(-3)), obligatorisk: true));

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

        // === Pulsundersökningar via domänens Skapa() ===
        // 1. Stängd enkät med frågor + responses (för resultatvy)
        var pulsMars = RegionHR.Pulse.Domain.PulseSurvey.Skapa(
            "Pulsundersokning mars 2026", "Manadsvis pulsmating av arbetsmiljo och trivsel.", "Admin");
        pulsMars.LaggTillFraga("Hur trivs du pa jobbet just nu?", 1);
        pulsMars.LaggTillFraga("Kanner du att din arbetsbelastning ar rimlig?", 2);
        pulsMars.LaggTillFraga("Har du tillgang till det stod du behover fran din chef?", 3);
        pulsMars.LaggTillFraga("Kanner du dig delaktig i beslut som paverkar ditt arbete?", 4);
        pulsMars.LaggTillFraga("Skulle du rekommendera din arbetsplats till en van?", 5);
        pulsMars.Oppna();
        pulsMars.Stang();

        // 3 anonyma svar
        var svar1 = RegionHR.Pulse.Domain.PulseSurveyResponse.Skapa(pulsMars.Id);
        foreach (var f in pulsMars.Fragor)
            svar1.LaggTillSvar(f.Id, f.Ordning <= 2 ? 4 : 3);
        var svar2 = RegionHR.Pulse.Domain.PulseSurveyResponse.Skapa(pulsMars.Id);
        foreach (var f in pulsMars.Fragor)
            svar2.LaggTillSvar(f.Id, f.Ordning <= 3 ? 5 : 4);
        var svar3 = RegionHR.Pulse.Domain.PulseSurveyResponse.Skapa(pulsMars.Id);
        foreach (var f in pulsMars.Fragor)
            svar3.LaggTillSvar(f.Id, f.Ordning == 1 ? 3 : 2);

        db.PulseSurveys.Add(pulsMars);
        db.PulseSurveyResponses.AddRange(svar1, svar2, svar3);

        // 2. Öppen enkät utan responses (för svarsflöde)
        var pulsApril = RegionHR.Pulse.Domain.PulseSurvey.Skapa(
            "Pulsundersokning april 2026", "Uppfoljning efter organisationsforandring.", "Admin");
        pulsApril.LaggTillFraga("Hur upplever du forandringarna pa din arbetsplats?", 1);
        pulsApril.LaggTillFraga("Har du fatt tillracklig information om forandringarna?", 2);
        pulsApril.LaggTillFraga("Kanner du dig trygg i din anstallning?", 3);
        pulsApril.Oppna();
        db.PulseSurveys.Add(pulsApril);

        // === Policyer via domänens Skapa() + Publicera() ===
        var policyGdpr = RegionHR.PolicyManagement.Domain.Policy.Skapa(
            "GDPR-policy", "Hantering av personuppgifter enligt GDPR och patientdatalagen.",
            RegionHR.PolicyManagement.Domain.PolicyCategory.GDPR, kraverBekraftelse: true, "Admin",
            sammanfattning: "Alla anstallda ska folja regionens regler for hantering av personuppgifter.");
        policyGdpr.Publicera();

        var policyIt = RegionHR.PolicyManagement.Domain.Policy.Skapa(
            "IT-sakerhetspolicy", "Regler for anvandning av IT-system, losenord och e-post.",
            RegionHR.PolicyManagement.Domain.PolicyCategory.ITSakerhet, kraverBekraftelse: true, "Admin",
            sammanfattning: "Alla anvandare av regionens IT-system ska folja dessa regler.");
        policyIt.Publicera();

        var policyArbetsmiljo = RegionHR.PolicyManagement.Domain.Policy.Skapa(
            "Arbetsmiljopolicy", "Regionens overgripande policy for fysisk och psykosocial arbetsmiljo.",
            RegionHR.PolicyManagement.Domain.PolicyCategory.Arbetsmiljo, kraverBekraftelse: false, "Admin",
            sammanfattning: "Arbetsmiljon ska vara saker och halsoframjande.");

        db.Policies.AddRange(policyGdpr, policyIt, policyArbetsmiljo);

        // Bekräftelser — kopplade till verkliga anställda
        db.PolicyConfirmations.AddRange(
            RegionHR.PolicyManagement.Domain.PolicyConfirmation.Skapa(policyGdpr.Id, employees[0].Id.Value, policyGdpr.Version), // Anna
            RegionHR.PolicyManagement.Domain.PolicyConfirmation.Skapa(policyGdpr.Id, employees[1].Id.Value, policyGdpr.Version), // Erik
            RegionHR.PolicyManagement.Domain.PolicyConfirmation.Skapa(policyGdpr.Id, employees[3].Id.Value, policyGdpr.Version), // Karl
            RegionHR.PolicyManagement.Domain.PolicyConfirmation.Skapa(policyIt.Id, employees[0].Id.Value, policyIt.Version),     // Anna
            RegionHR.PolicyManagement.Domain.PolicyConfirmation.Skapa(policyIt.Id, employees[2].Id.Value, policyIt.Version));    // Maria

        // === Timesheets via domänens Skapa() + RegistreraTimmar() + SkickaIn() etc. ===
        // Anna — Inskickad (mars)
        var tsAnnaMars = RegionHR.Scheduling.Domain.Timesheet.Skapa(employees[0].Id.Value, 2026, 3, 160m);
        tsAnnaMars.RegistreraTimmar(168.5m, 8.5m);
        tsAnnaMars.SkickaIn();

        // Erik — Inskickad (mars)
        var tsErikMars = RegionHR.Scheduling.Domain.Timesheet.Skapa(employees[1].Id.Value, 2026, 3, 160m);
        tsErikMars.RegistreraTimmar(162m, 2m);
        tsErikMars.SkickaIn();

        // Karl — Godkänd (februari)
        var tsKarlFeb = RegionHR.Scheduling.Domain.Timesheet.Skapa(employees[3].Id.Value, 2026, 2, 160m);
        tsKarlFeb.RegistreraTimmar(158m, 0m);
        tsKarlFeb.SkickaIn();
        tsKarlFeb.Godkann(employees[7].Id.Value, "Ser bra ut"); // Anders godkänner

        // Maria — Avslagen (mars)
        var tsMariaMars = RegionHR.Scheduling.Domain.Timesheet.Skapa(employees[2].Id.Value, 2026, 3, 120m);
        tsMariaMars.RegistreraTimmar(135m, 15m);
        tsMariaMars.SkickaIn();
        tsMariaMars.Avvisa(employees[7].Id.Value, "Overtid ej forhandsanmald"); // Anders avvisar

        // Sara — Öppen (mars, ej inskickad)
        var tsSaraMars = RegionHR.Scheduling.Domain.Timesheet.Skapa(employees[4].Id.Value, 2026, 3, 120m);
        tsSaraMars.RegistreraTimmar(100m, 0m);

        db.Timesheets.AddRange(tsAnnaMars, tsErikMars, tsKarlFeb, tsMariaMars, tsSaraMars);

        // === TravelClaims via domänens Skapa() ===
        var reseErik = RegionHR.Travel.Domain.TravelClaim.Skapa(
            employees[1].Id, "Konferens Stockholm", DateOnly.FromDateTime(DateTime.Today.AddDays(-10)));
        reseErik.SattTraktamente(2, 0);
        reseErik.SkickaIn();
        var reseAnna = RegionHR.Travel.Domain.TravelClaim.Skapa(
            employees[0].Id, "Utbildning Goteborg", DateOnly.FromDateTime(DateTime.Today.AddDays(-25)));
        reseAnna.SattTraktamente(1, 0);
        reseAnna.SkickaIn();
        reseAnna.Attestera("Eva Nilsson");
        db.TravelClaims.AddRange(reseErik, reseAnna);

        // === DelegatedAccess via domänens Skapa() ===
        var delegEva = RegionHR.Infrastructure.Authorization.DelegatedAccess.Skapa(
            employees[8].Id.Value, employees[2].Id.Value, "Ledighet – godkanna",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(14)), "Semester");
        var delegAnders = RegionHR.Infrastructure.Authorization.DelegatedAccess.Skapa(
            employees[7].Id.Value, employees[3].Id.Value, "Tidrapporter – attestera",
            DateOnly.FromDateTime(DateTime.Today.AddDays(10)), DateOnly.FromDateTime(DateTime.Today.AddDays(24)), "Konferens");
        db.DelegatedAccesses.AddRange(delegEva, delegAnders);

        // === ShiftSwapRequests via domänens Skapa() ===
        var swap1 = RegionHR.Scheduling.Domain.ShiftSwapRequest.Skapa(
            employees[0].Id, Guid.NewGuid(), "Behover byta pga lakartid");
        swap1.Erbjud(employees[4].Id);
        swap1.Acceptera(employees[4].Id, null);
        var swap2 = RegionHR.Scheduling.Domain.ShiftSwapRequest.Skapa(
            employees[3].Id, Guid.NewGuid(), "Vill byta till dagpass");
        swap2.Erbjud(employees[6].Id);
        db.ShiftSwapRequests.AddRange(swap1, swap2);

        // === WellnessClaims via domänens Skapa() ===
        var friskvardAnna = RegionHR.Wellness.Domain.WellnessClaim.Skapa(
            employees[0].Id.Value, "Personlig traning", 2000m, DateOnly.FromDateTime(DateTime.Today.AddDays(-15)));
        friskvardAnna.Godkann(employees[8].Id.Value, "Godkant");
        var friskvardKarl = RegionHR.Wellness.Domain.WellnessClaim.Skapa(
            employees[3].Id.Value, "Yoga", 1500m, DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));
        var friskvardErik = RegionHR.Wellness.Domain.WellnessClaim.Skapa(
            employees[1].Id.Value, "Simkort arskort", 3500m, DateOnly.FromDateTime(DateTime.Today.AddDays(-30)));
        friskvardErik.Godkann(employees[8].Id.Value);
        db.WellnessClaims.AddRange(friskvardAnna, friskvardKarl, friskvardErik);

        // === Announcements via domänens Skapa() ===
        var annons1 = RegionHR.Communication.Domain.Announcement.Skapa(
            "Parkering under ombyggnation", "Parkeringshuset stängs 1-15 april för renovering. Använd alternativ parkering vid norra infarten.",
            RegionHR.Communication.Domain.AnnouncementPriority.Viktig, "Admin");
        annons1.Publicera();
        var annons2 = RegionHR.Communication.Domain.Announcement.Skapa(
            "Vaccinering influensa", "Årets influensavaccinering erbjuds 1-30 november. Boka tid via MinSida.",
            RegionHR.Communication.Domain.AnnouncementPriority.Normal, "Admin");
        annons2.Publicera();
        var annons3 = RegionHR.Communication.Domain.Announcement.Skapa(
            "IT-driftstopp planerat 5 april", "IT-system nedstängda 02:00-06:00 för uppgradering.",
            RegionHR.Communication.Domain.AnnouncementPriority.Kritisk, "Admin");
        db.Announcements.AddRange(annons1, annons2, annons3);

        // === InsuranceCoverages ===
        db.InsuranceCoverages.AddRange(
            RegionHR.Insurance.Domain.InsuranceCoverage.Skapa(RegionHR.Insurance.Domain.InsuranceType.TGL, "TGL — Tjänstegrupplivförsäkring", "KPA Pension", "Livförsäkring för alla anställda"),
            RegionHR.Insurance.Domain.InsuranceCoverage.Skapa(RegionHR.Insurance.Domain.InsuranceType.AGS, "AGS — Avtalsgruppsjukförsäkring", "AFA Försäkring", "Kompletterande sjukförsäkring"),
            RegionHR.Insurance.Domain.InsuranceCoverage.Skapa(RegionHR.Insurance.Domain.InsuranceType.TFA, "TFA — Trygghetsförsäkring vid arbetsskada", "AFA Försäkring", "Arbetsskadeförsäkring"),
            RegionHR.Insurance.Domain.InsuranceCoverage.Skapa(RegionHR.Insurance.Domain.InsuranceType.AFA, "AFA — Avtalsförsäkring", "AFA Försäkring", "Samlingsbegrepp"),
            RegionHR.Insurance.Domain.InsuranceCoverage.Skapa(RegionHR.Insurance.Domain.InsuranceType.PSA, "PSA — Avtal om ersättning vid personskada", "AFA Försäkring", "Personskadeförsäkring"));

        // === Recognitions ===
        db.Recognitions.AddRange(
            RegionHR.Communication.Domain.Recognition.Skapa(employees[0].Id.Value, employees[3].Id.Value, "Samarbete", "Karl hoppade in pa kort varsel och tacklade kvallspasset. Fantastiskt!"),
            RegionHR.Communication.Domain.Recognition.Skapa(employees[1].Id.Value, employees[6].Id.Value, "Innovation", "Helena foreslog nytt triageflode som kortat vantetiderna."),
            RegionHR.Communication.Domain.Recognition.Skapa(employees[4].Id.Value, employees[0].Id.Value, "Hjalpsamhet", "Anna tog sig tid att handleda en ny kollega trots hog belastning."));

        // === SuccessionPlans ===
        db.SuccessionPlans.AddRange(
            RegionHR.Positions.Domain.SuccessionPlan.Skapa(posVc.Id, employees[7].Id.Value, 2029, employees[0].Id.Value, RegionHR.Positions.Domain.SuccessionReadiness.RedoInom1Ar, 75),
            RegionHR.Positions.Domain.SuccessionPlan.Skapa(posLakAkut.Id, employees[1].Id.Value, 2032, null, RegionHR.Positions.Domain.SuccessionReadiness.EjIdentifierad, 0),
            RegionHR.Positions.Domain.SuccessionPlan.Skapa(posSsk32.Id, employees[0].Id.Value, 2035, employees[4].Id.Value, RegionHR.Positions.Domain.SuccessionReadiness.RedoInom2Ar, 40));

        // === FeedbackRound + Responses ===
        var fb360Anna = RegionHR.Performance.Domain.FeedbackRound.Skapa(employees[0].Id.Value, "360-feedback Anna Svensson 2026");
        fb360Anna.Oppna();
        fb360Anna.Stang();
        var fbResp1 = RegionHR.Performance.Domain.FeedbackResponse.Skapa(fb360Anna.Id, employees[7].Id.Value, "Chef", 4, "Bra samarbetsformaga");
        var fbResp2 = RegionHR.Performance.Domain.FeedbackResponse.Skapa(fb360Anna.Id, employees[3].Id.Value, "Kollega", 5, "Alltid hjalpsam");
        var fbResp3 = RegionHR.Performance.Domain.FeedbackResponse.Skapa(fb360Anna.Id, employees[4].Id.Value, "Kollega", 4);

        var fb360Erik = RegionHR.Performance.Domain.FeedbackRound.Skapa(employees[1].Id.Value, "360-feedback Erik Johansson 2026");
        fb360Erik.Oppna();
        db.FeedbackRounds.AddRange(fb360Anna, fb360Erik);
        db.FeedbackResponses.AddRange(fbResp1, fbResp2, fbResp3);

        // === LASAccumulations ===
        var lasMaria = RegionHR.LAS.Domain.LASAccumulation.Skapa(employees[2].Id, EmploymentType.SAVA);
        lasMaria.LaggTillPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(-200)), DateOnly.FromDateTime(DateTime.Today.AddDays(-50)), null);
        lasMaria.LaggTillPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(-40)), DateOnly.FromDateTime(DateTime.Today), null);
        var lasJohan = RegionHR.LAS.Domain.LASAccumulation.Skapa(employees[5].Id, EmploymentType.Vikariat);
        lasJohan.LaggTillPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(-320)), DateOnly.FromDateTime(DateTime.Today), null);
        db.LASAccumulations.AddRange(lasMaria, lasJohan);

        // === Vacancies ===
        var vakSsk = RegionHR.Recruitment.Domain.Vacancy.Skapa(
            avd32.Id, "Sjukskoterska", "Vi soker erfaren sjukskoterska till Avdelning 32.",
            EmploymentType.Tillsvidare, DateOnly.FromDateTime(DateTime.Today.AddDays(30)));
        vakSsk.Publicera(false, false);
        vakSsk.TaEmotAnsokan("Anna Bergman", "anna.bergman@mail.se", null);
        vakSsk.TaEmotAnsokan("Karl Lindqvist", "karl.l@mail.se", null);
        var vakUsk = RegionHR.Recruitment.Domain.Vacancy.Skapa(
            iva.Id, "Underskoterska IVA", "IVA soker underskoterska med intensivvardserfarenhet.",
            EmploymentType.Tillsvidare, DateOnly.FromDateTime(DateTime.Today.AddDays(14)));
        vakUsk.Publicera(false, false);
        var vakLak = RegionHR.Recruitment.Domain.Vacancy.Skapa(
            akuten.Id, "Lakare akutmottagningen", "Specialist i akutsjukvard.",
            EmploymentType.Tillsvidare, DateOnly.FromDateTime(DateTime.Today.AddDays(60)));
        db.Vacancies.AddRange(vakSsk, vakUsk, vakLak);

        // === MandatoryTrainings ===
        db.MandatoryTrainings.AddRange(
            RegionHR.Competence.Domain.MandatoryTraining.Skapa("Sjukskoterska", "HLR", 12, "Hjartstoppsutbildning"),
            RegionHR.Competence.Domain.MandatoryTraining.Skapa("Sjukskoterska", "Brandskydd", 24, "Grundutbildning brandsakerhet"),
            RegionHR.Competence.Domain.MandatoryTraining.Skapa("Alla", "Hygien", 12, "Basala hygienrutiner"),
            RegionHR.Competence.Domain.MandatoryTraining.Skapa("Underskoterska", "Forsta hjalpen", 24));

        // === ReferenceChecks ===
        var refCheck1 = RegionHR.Recruitment.Domain.ReferenceCheck.Skapa(vakSsk.Id, "Anna Bergman", "Lars Svensson", "Tidigare chef");
        refCheck1.Genomfor("Mycket positiva omdomen. Rekommenderas starkt.", true);
        var refCheck2 = RegionHR.Recruitment.Domain.ReferenceCheck.Skapa(vakSsk.Id, "Karl Lindqvist", "Eva Holm", "Handledare");
        db.ReferenceChecks.AddRange(refCheck1, refCheck2);

        // === MBL-förhandlingar ===
        var mbl1 = RegionHR.CaseManagement.Domain.MBLNegotiation.Skapa("Omorganisation Avdelning 32", RegionHR.CaseManagement.Domain.MBLType.Forhandling, DateOnly.FromDateTime(DateTime.Today.AddDays(-5)), "Kommunal — Anna Ek", "HR-chef Eva Nilsson");
        mbl1.Paborja();
        mbl1.Avsluta();
        mbl1.RegistreraProtokoll("Parterna enade. Omorganisation genomfors fran 1 maj 2026.");
        var mbl2 = RegionHR.CaseManagement.Domain.MBLNegotiation.Skapa("Nyanstallning verksamhetschef", RegionHR.CaseManagement.Domain.MBLType.Information, DateOnly.FromDateTime(DateTime.Today.AddDays(7)), "Vardforbundet — Karl Berg", "HR-chef Eva Nilsson");
        db.MBLNegotiations.AddRange(mbl1, mbl2);

        // === SalaryCodes via SalaryCodeSeed ===
        db.SalaryCodes.AddRange(RegionHR.Payroll.Domain.SalaryCodeSeed.GetAll());

        // === Automation Framework ===
        var catCompliance = RegionHR.Automation.Domain.AutomationCategory.Skapa("Compliance", "Lagefterlevnad och regelkrav (LAS, ATL, diskriminering)", "Gavel");
        var catFranvaro = RegionHR.Automation.Domain.AutomationCategory.Skapa("Franvaro", "Sjukfranvaro, VAB och frånvarohantering", "EventBusy");
        var catLon = RegionHR.Automation.Domain.AutomationCategory.Skapa("Lon", "Lonebearbetning och utbetalning", "Payments");
        var catKompetens = RegionHR.Automation.Domain.AutomationCategory.Skapa("Kompetens", "Certifieringar, utbildning och kompetensgap", "School");
        var catRekrytering = RegionHR.Automation.Domain.AutomationCategory.Skapa("Rekrytering", "Rekrytering, onboarding och offboarding", "PersonAdd");
        var catGDPR = RegionHR.Automation.Domain.AutomationCategory.Skapa("GDPR", "Dataskydd, gallring och registerutdrag", "Security");
        db.AutomationCategories.AddRange(catCompliance, catFranvaro, catLon, catKompetens, catRekrytering, catGDPR);

        // --- Automation Rules (22 regler) ---
        // Compliance (5)
        var rules = new List<RegionHR.Automation.Domain.AutomationRule>
        {
            RegionHR.Automation.Domain.AutomationRule.Skapa("LAS-varning 300 dagar", catCompliance.Id, "LASAccumulationUpdated", "{\"dagar_min\":300}", "{\"typ\":\"notify\",\"mall\":\"las_varning\"}", RegionHR.Automation.Domain.AutomationLevel.Notify, true),
            RegionHR.Automation.Domain.AutomationRule.Skapa("LAS konvertering 360 dagar", catCompliance.Id, "LASAccumulationUpdated", "{\"dagar_min\":360}", "{\"typ\":\"autopilot\",\"mall\":\"las_konvertering\"}", RegionHR.Automation.Domain.AutomationLevel.Notify, true),
            RegionHR.Automation.Domain.AutomationRule.Skapa("ATL max veckoarbetstid", catCompliance.Id, "ShiftCreated", "{\"max_timmar_vecka\":48}", "{\"typ\":\"block\"}", RegionHR.Automation.Domain.AutomationLevel.Block, true),
            RegionHR.Automation.Domain.AutomationRule.Skapa("ATL dygnsvila 11 timmar", catCompliance.Id, "ShiftCreated", "{\"min_vila_timmar\":11}", "{\"typ\":\"block\"}", RegionHR.Automation.Domain.AutomationLevel.Block, true),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Diskrimineringskontroll loneoversyn", catCompliance.Id, "SalaryReviewCreated", "{\"avvikelse_procent\":5}", "{\"typ\":\"suggest\",\"mall\":\"diskriminering_varning\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),

            // Franvaro (4)
            RegionHR.Automation.Domain.AutomationRule.Skapa("FK-anmalan dag 15", catFranvaro.Id, "SickLeaveUpdated", "{\"dagar_min\":14}", "{\"typ\":\"notify\",\"mall\":\"fk_anmalan\"}", RegionHR.Automation.Domain.AutomationLevel.Notify, true),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Rehab-trigger dag 30", catFranvaro.Id, "SickLeaveUpdated", "{\"dagar_min\":30}", "{\"typ\":\"suggest\",\"mall\":\"rehab_start\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Sjukfranvaro-eskalering dag 90", catFranvaro.Id, "SickLeaveUpdated", "{\"dagar_min\":90}", "{\"typ\":\"notify\",\"mall\":\"eskalering_90\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Korttidsfranvaro-monster", catFranvaro.Id, "CronDaily", "{\"max_tillfallen_6_man\":6}", "{\"typ\":\"suggest\",\"mall\":\"korttid_monster\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),

            // Lon (4)
            RegionHR.Automation.Domain.AutomationRule.Skapa("AGI-generering manadsslut", catLon.Id, "PayrollRunCompleted", "{}", "{\"typ\":\"autopilot\",\"mall\":\"agi_xml\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Lonefil pain.001", catLon.Id, "PayrollRunApproved", "{}", "{\"typ\":\"autopilot\",\"mall\":\"pain001\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Loneavvikelse-varning", catLon.Id, "PayrollCalculated", "{\"avvikelse_procent\":20}", "{\"typ\":\"suggest\",\"mall\":\"lon_avvikelse\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationRule.Skapa("OB-tillagg automatisk berakning", catLon.Id, "TimesheetApproved", "{}", "{\"typ\":\"autopilot\",\"mall\":\"ob_berakning\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),

            // Kompetens (3)
            RegionHR.Automation.Domain.AutomationRule.Skapa("Certifiering utgar 30 dagar", catKompetens.Id, "CronDaily", "{\"dagar_kvar\":30}", "{\"typ\":\"notify\",\"mall\":\"cert_utgar_30\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Certifiering utgar 90 dagar", catKompetens.Id, "CronDaily", "{\"dagar_kvar\":90}", "{\"typ\":\"notify\",\"mall\":\"cert_utgar_90\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Obligatorisk utbildning forsenad", catKompetens.Id, "CronWeekly", "{\"forsenad\":true}", "{\"typ\":\"suggest\",\"mall\":\"utbildning_paminnelse\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),

            // Rekrytering (3)
            RegionHR.Automation.Domain.AutomationRule.Skapa("Onboarding auto-start", catRekrytering.Id, "EmploymentCreated", "{}", "{\"typ\":\"autopilot\",\"mall\":\"onboarding_start\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Offboarding auto-start", catRekrytering.Id, "EmploymentTerminated", "{}", "{\"typ\":\"autopilot\",\"mall\":\"offboarding_start\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Provanstallning utgar", catRekrytering.Id, "CronDaily", "{\"dagar_kvar\":30}", "{\"typ\":\"notify\",\"mall\":\"provanstallning_utgar\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),

            // GDPR (3)
            RegionHR.Automation.Domain.AutomationRule.Skapa("Retention gallring 7 ar", catGDPR.Id, "CronMonthly", "{\"ar\":7}", "{\"typ\":\"autopilot\",\"mall\":\"gallring\"}", RegionHR.Automation.Domain.AutomationLevel.Autopilot, true),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Registerutdrag auto-generering", catGDPR.Id, "DataSubjectRequestCreated", "{}", "{\"typ\":\"autopilot\",\"mall\":\"registerutdrag\"}", RegionHR.Automation.Domain.AutomationLevel.Autopilot, true),
            RegionHR.Automation.Domain.AutomationRule.Skapa("Samtycke utgangen", catGDPR.Id, "CronWeekly", "{\"dagar_kvar\":0}", "{\"typ\":\"notify\",\"mall\":\"samtycke_utgangen\"}", RegionHR.Automation.Domain.AutomationLevel.Notify),
        };
        db.AutomationRules.AddRange(rules);

        // --- Default LevelConfigs (6 st) ---
        db.AutomationLevelConfigs.AddRange(
            RegionHR.Automation.Domain.AutomationLevelConfig.Skapa(catCompliance.Id, RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationLevelConfig.Skapa(catFranvaro.Id, RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationLevelConfig.Skapa(catLon.Id, RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationLevelConfig.Skapa(catKompetens.Id, RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationLevelConfig.Skapa(catRekrytering.Id, RegionHR.Automation.Domain.AutomationLevel.Notify),
            RegionHR.Automation.Domain.AutomationLevelConfig.Skapa(catGDPR.Id, RegionHR.Automation.Domain.AutomationLevel.Autopilot));

        // === KPI Definitions (10 st) ===
        var kpiHeadcount = KPIDefinition.Skapa("Headcount", "Workforce", "COUNT(employees WHERE active=true)", "count", "HigherIsBetter", 100, 80, 50);
        var kpiFTE = KPIDefinition.Skapa("FTE (heltidsekvivalenter)", "Workforce", "SUM(employment_percent)/100", "count", "HigherIsBetter", 95, 85, 70);
        var kpiVacancy = KPIDefinition.Skapa("Vakansgrad", "Recruitment", "vacant_positions/total_positions*100", "percent", "LowerIsBetter", 5, 10, 15);
        var kpiTurnover = KPIDefinition.Skapa("Personalomsattning", "Turnover", "terminated_12m/avg_headcount*100", "percent", "LowerIsBetter", 8, 15, 20);
        var kpiSickLeave = KPIDefinition.Skapa("Sjukfranvaro %", "Absence", "sick_days/total_workdays*100", "percent", "LowerIsBetter", 4, 6, 8);
        var kpiSalaryCost = KPIDefinition.Skapa("Lonekostnad per FTE", "Compensation", "total_salary_cost/fte_count", "currency", "LowerIsBetter", 45000, 50000, 55000);
        var kpiCertCoverage = KPIDefinition.Skapa("Certifieringstackning", "Competence", "valid_certs/required_certs*100", "percent", "HigherIsBetter", 95, 85, 70);
        var kpiTimeToFill = KPIDefinition.Skapa("Time to fill (dagar)", "Recruitment", "AVG(fill_date - publish_date)", "days", "LowerIsBetter", 30, 45, 60);
        var kpiENPS = KPIDefinition.Skapa("eNPS", "Engagement", "promoters_pct - detractors_pct", "count", "HigherIsBetter", 30, 10, -10);
        var kpiLASRisk = KPIDefinition.Skapa("LAS-riskantal", "Compliance", "COUNT(las_days >= 300)", "count", "LowerIsBetter", 0, 3, 5);
        db.KPIDefinitions.AddRange(kpiHeadcount, kpiFTE, kpiVacancy, kpiTurnover, kpiSickLeave, kpiSalaryCost, kpiCertCoverage, kpiTimeToFill, kpiENPS, kpiLASRisk);

        // === KPI Snapshots (current period: 2026-Q1) ===
        db.KPISnapshots.AddRange(
            KPISnapshot.Skapa(kpiHeadcount.Id, "2026-Q1", 10, 10, "Stable"),
            KPISnapshot.Skapa(kpiFTE.Id, "2026-Q1", 9.2m, 9.0m, "Up"),
            KPISnapshot.Skapa(kpiVacancy.Id, "2026-Q1", 7.5m, 8.0m, "Down"),
            KPISnapshot.Skapa(kpiTurnover.Id, "2026-Q1", 0, null, "Stable"),
            KPISnapshot.Skapa(kpiSickLeave.Id, "2026-Q1", 4.2m, 4.5m, "Down"),
            KPISnapshot.Skapa(kpiSalaryCost.Id, "2026-Q1", 42500, 41000, "Up"),
            KPISnapshot.Skapa(kpiCertCoverage.Id, "2026-Q1", 88, 85, "Up"),
            KPISnapshot.Skapa(kpiTimeToFill.Id, "2026-Q1", 0, null, "Stable"),
            KPISnapshot.Skapa(kpiENPS.Id, "2026-Q1", 0, null, "Stable"),
            KPISnapshot.Skapa(kpiLASRisk.Id, "2026-Q1", 2, 1, "Up"));

        // === Prediction Models (4 st) ===
        var pmAttrition = PredictionModel.Skapa("Uppsagningsriskmodell", "Attrition", """{"features":["tenure","age","salary_delta","sick_days","overtime_hours"]}""");
        pmAttrition.UppdateraTranning(0.82m);
        var pmHeadcount = PredictionModel.Skapa("Bemanningsprognos", "HeadcountForecast", """{"features":["headcount_history","turnover_rate","planned_recruitment"]}""");
        pmHeadcount.UppdateraTranning(0.91m);
        var pmSickLeave = PredictionModel.Skapa("Sjukfranvaroprognos", "SickLeaveForecast", """{"features":["season","sick_history","workload","age"]}""");
        pmSickLeave.UppdateraTranning(0.78m);
        var pmLaborCost = PredictionModel.Skapa("Lonekostnadsprognos", "LaborCostForecast", """{"features":["salary_base","ob_tillagg","overtime","employer_fees","pension"]}""");
        pmLaborCost.UppdateraTranning(0.95m);
        db.PredictionModels.AddRange(pmAttrition, pmHeadcount, pmSickLeave, pmLaborCost);

        // === VMS / Contingent Workforce ===
        var vendor1 = RegionHR.VMS.Domain.Vendor.Skapa("MedStaff AB", "556789-1234", "Anna Lind", "anna@medstaff.se", "031-111222", "Sjukvard");
        var vendor2 = RegionHR.VMS.Domain.Vendor.Skapa("VardPool Sverige", "556790-5678", "Erik Johansson", "erik@vardpool.se", "08-333444", "Sjukvard");
        var vendor3 = RegionHR.VMS.Domain.Vendor.Skapa("BemanningsExpert", "556791-9012", "Lisa Berg", "lisa@bemanningsexpert.se", "040-555666", "IT");
        db.Vendors.AddRange(vendor1, vendor2, vendor3);

        var ramavtal1 = RegionHR.VMS.Domain.FrameworkAgreement.Skapa(
            vendor1.Id, new DateOnly(2025, 1, 1), new DateOnly(2027, 12, 31),
            "Standardvillkor for bemanningssjukskoterskor", 3, "Automatisk forlangning 12 manader", 5_000_000m);
        ramavtal1.LaggTillRateCard("Sjukskoterska", 520m, 85m, 195m, 25m);
        ramavtal1.LaggTillRateCard("Underskoterska", 380m, 65m, 145m, 25m);

        var ramavtal2 = RegionHR.VMS.Domain.FrameworkAgreement.Skapa(
            vendor3.Id, new DateOnly(2025, 6, 1), new DateOnly(2026, 12, 31),
            "IT-konsulter systemutveckling", 2, "Ingen automatisk forlangning", 2_000_000m);
        ramavtal2.LaggTillRateCard("Systemutvecklare", 950m, 0m, 0m, 25m);
        ramavtal2.LaggTillRateCard("Projektledare", 1100m, 0m, 0m, 25m);
        db.FrameworkAgreements.AddRange(ramavtal1, ramavtal2);

        var vmsRequest = RegionHR.VMS.Domain.StaffingRequest.Skapa(
            avd32.Id, "Sjukskoterska", new DateOnly(2026, 1, 15), new DateOnly(2026, 6, 30), 2,
            "Minst 3 ars erfarenhet inom akutsjukvard");
        vmsRequest.SkickaIn();
        vmsRequest.Godkann();
        vmsRequest.Tillsatt();
        db.StaffingRequests.Add(vmsRequest);

        var cw1 = RegionHR.VMS.Domain.ContingentWorker.Skapa(
            "Maria Lindqvist", vendor1.Id, vmsRequest.Id, new DateOnly(2026, 1, 20), new DateOnly(2026, 6, 30), 520m, avd32.Id);
        var cw2 = RegionHR.VMS.Domain.ContingentWorker.Skapa(
            "Johan Pettersson", vendor1.Id, vmsRequest.Id, new DateOnly(2026, 2, 1), null, 520m, avd32.Id);
        db.ContingentWorkers.AddRange(cw1, cw2);

        var tr1 = RegionHR.VMS.Domain.ContingentTimeReport.Skapa(cw1.Id, "2026-02", 152m, 8m, 4m);
        tr1.SkickaIn();
        tr1.Attestera(Guid.NewGuid());
        var tr2 = RegionHR.VMS.Domain.ContingentTimeReport.Skapa(cw2.Id, "2026-02", 160m, 0m, 0m);
        tr2.SkickaIn();
        var tr3 = RegionHR.VMS.Domain.ContingentTimeReport.Skapa(cw1.Id, "2026-03", 168m, 12m, 8m);
        db.ContingentTimeReports.AddRange(tr1, tr2, tr3);

        var inv1 = RegionHR.VMS.Domain.VendorInvoice.Skapa(vendor1.Id, "2026-02", 168_480m);
        inv1.Matcha(165_100m);
        inv1.Godkann();
        var inv2 = RegionHR.VMS.Domain.VendorInvoice.Skapa(vendor1.Id, "2026-03", 183_040m);
        db.VendorInvoices.AddRange(inv1, inv2);

        db.VendorPerformances.AddRange(
            RegionHR.VMS.Domain.VendorPerformance.Skapa(vendor1.Id, "2026-Q1", 4, "Bra leverans, punktliga rapporter"),
            RegionHR.VMS.Domain.VendorPerformance.Skapa(vendor2.Id, "2026-Q1", 3, "Acceptabelt men forlangda ledtider"),
            RegionHR.VMS.Domain.VendorPerformance.Skapa(vendor3.Id, "2026-Q1", 5, "Utmarkt kvalitet, proaktiv kommunikation"));

        db.SpendCategories.AddRange(
            RegionHR.VMS.Domain.SpendCategory.Skapa("Bemanningssjukskoterska", "Inhyrda sjukskoterskor via bemanningsforetag"),
            RegionHR.VMS.Domain.SpendCategory.Skapa("IT-konsult", "Externt inhyrda IT-resurser"),
            RegionHR.VMS.Domain.SpendCategory.Skapa("Administrativ", "Administrativ stodpersonal via bemanning"));

        // === WFM: DemandPatterns for Akuten (5 st — vardagar, helger) ===
        db.DemandPatterns.AddRange(
            new RegionHR.Scheduling.Domain.DemandPattern { EnhetId = akuten.Id, Veckodag = 1, GenomsnittligBelastning = 12, SasongsVariation = 1.0m }, // Mån
            new RegionHR.Scheduling.Domain.DemandPattern { EnhetId = akuten.Id, Veckodag = 2, GenomsnittligBelastning = 11, SasongsVariation = 1.0m }, // Tis
            new RegionHR.Scheduling.Domain.DemandPattern { EnhetId = akuten.Id, Veckodag = 5, GenomsnittligBelastning = 14, SasongsVariation = 1.1m }, // Fre
            new RegionHR.Scheduling.Domain.DemandPattern { EnhetId = akuten.Id, Veckodag = 6, GenomsnittligBelastning = 16, SasongsVariation = 1.2m }, // Lör
            new RegionHR.Scheduling.Domain.DemandPattern { EnhetId = akuten.Id, Veckodag = 0, GenomsnittligBelastning = 15, SasongsVariation = 1.15m }); // Sön

        // === WFM: DemandEvents (3 st) ===
        db.DemandEvents.AddRange(
            new RegionHR.Scheduling.Domain.DemandEvent { Namn = "Jul 2026", Typ = "Helgdag", PaverkanGrad = 1.6m, DatumFran = new DateOnly(2026, 12, 23), DatumTill = new DateOnly(2026, 12, 26) },
            new RegionHR.Scheduling.Domain.DemandEvent { Namn = "Midsommar 2026", Typ = "Helgdag", PaverkanGrad = 1.4m, DatumFran = new DateOnly(2026, 6, 19), DatumTill = new DateOnly(2026, 6, 21) },
            new RegionHR.Scheduling.Domain.DemandEvent { Namn = "Influensasasong 2026/2027", Typ = "Influensasasong", PaverkanGrad = 1.35m, DatumFran = new DateOnly(2026, 11, 1), DatumTill = new DateOnly(2027, 2, 28) });

        // === WFM: SchedulingConstraints (5 st) ===
        db.SchedulingConstraints.AddRange(
            new RegionHR.Scheduling.Domain.SchedulingConstraint { Typ = "ATL", Beskrivning = "ATL §13: Minst 11h sammanhangande dygnsvila", Vikt = 100, ArHard = true },
            new RegionHR.Scheduling.Domain.SchedulingConstraint { Typ = "ATL", Beskrivning = "ATL §14: Minst 36h sammanhangande veckovila", Vikt = 100, ArHard = true },
            new RegionHR.Scheduling.Domain.SchedulingConstraint { Typ = "Kompetens", Beskrivning = "Ratt kompetens pa varje pass (legitimation, HLR etc.)", Vikt = 95, ArHard = true },
            new RegionHR.Scheduling.Domain.SchedulingConstraint { Typ = "Preferens", Beskrivning = "Medarbetarpreferenser for pass och tider", Vikt = 30, ArHard = false },
            new RegionHR.Scheduling.Domain.SchedulingConstraint { Typ = "Kostnad", Beskrivning = "Minimera OB- och overtidskostnader", Vikt = 40, ArHard = false });

        // === WFM: EmployeeAvailability (4 st) ===
        db.EmployeeAvailabilities.AddRange(
            new RegionHR.Scheduling.Domain.EmployeeAvailability { AnstallId = employees[0].Id, Veckodag = 1, TidFran = new TimeOnly(7, 0), TidTill = new TimeOnly(16, 0), Preferens = "VillJobba", ArRepeterande = true },
            new RegionHR.Scheduling.Domain.EmployeeAvailability { AnstallId = employees[0].Id, Veckodag = 6, Preferens = "KanInte", ArRepeterande = true },
            new RegionHR.Scheduling.Domain.EmployeeAvailability { AnstallId = employees[2].Id, Veckodag = 5, TidFran = new TimeOnly(15, 0), TidTill = new TimeOnly(22, 0), Preferens = "KanJobba", ArRepeterande = true },
            new RegionHR.Scheduling.Domain.EmployeeAvailability { AnstallId = employees[3].Id, Datum = new DateOnly(2026, 4, 10), Preferens = "KanInte", ArRepeterande = false });

        // === WFM: FatigueScores (2 st for seeded employees) ===
        db.FatigueScores.AddRange(
            RegionHR.Scheduling.Domain.FatigueScore.Berakna(employees[1].Id, konsekutivaDagar: 5, nattpassSenaste7Dagar: 2, totalTimmarSenaste7Dagar: 42.5m, kortVila: 1, helgarbeteSenaste4Veckor: 3),
            RegionHR.Scheduling.Domain.FatigueScore.Berakna(employees[3].Id, konsekutivaDagar: 3, nattpassSenaste7Dagar: 1, totalTimmarSenaste7Dagar: 35.0m, kortVila: 0, helgarbeteSenaste4Veckor: 1));

        await db.SaveChangesAsync();
    }
}
