using Microsoft.EntityFrameworkCore;
using RegionHR.Core.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(RegionHRDbContext db)
    {
        if (await db.Employees.AnyAsync()) return; // Already seeded

        // Organization units
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

        // Employees (10 realistic Swedish employees) with employments
        var seedEmployees = new (string Fornamn, string Efternamn, string Pnr, string Befattning, OrganizationId Enhet, Money Lon)[]
        {
            ("Anna", "Svensson", "19850315-2345", "Sjukskoterska", avd32.Id, Money.SEK(34500m)),
            ("Erik", "Johansson", "19780622-1230", "Lakare", akuten.Id, Money.SEK(62000m)),
            ("Maria", "Lindgren", "19900101-5674", "Underskoterska", avd33.Id, Money.SEK(27800m)),
            ("Karl", "Berg", "19820714-3450", "Sjukskoterska", iva.Id, Money.SEK(35200m)),
            ("Sara", "Karlsson", "19950430-7896", "Underskoterska", avd32.Id, Money.SEK(27500m)),
            ("Johan", "Nilsson", "19880215-2344", "Lakare", akuten.Id, Money.SEK(58000m)),
            ("Helena", "Bergstrom", "19920918-4562", "Sjukskoterska", avd33.Id, Money.SEK(33800m)),
            ("Anders", "Olsson", "19750305-6786", "Verksamhetschef", sjukhus.Id, Money.SEK(52000m)),
            ("Eva", "Nilsson", "19800712-1350", "HR-chef", sjukhus.Id, Money.SEK(48000m)),
            ("Per", "Andersson", "19870523-2460", "Underskoterska", iva.Id, Money.SEK(28200m)),
        };

        foreach (var (fornamn, efternamn, pnr, befattning, enhetId, lon) in seedEmployees)
        {
            var employee = Employee.Skapa(
                new Personnummer(pnr),
                fornamn,
                efternamn);

            employee.UppdateraKontaktuppgifter(
                $"{fornamn.ToLower()}.{efternamn.ToLower()}@regionvg.se",
                $"070-{Random.Shared.Next(100, 999)} {Random.Shared.Next(10, 99)} {Random.Shared.Next(10, 99)}",
                null);

            var startdatum = DateOnly.FromDateTime(DateTime.Today.AddYears(-Random.Shared.Next(1, 15)));

            employee.LaggTillAnstallning(
                enhetId,
                EmploymentType.Tillsvidare,
                CollectiveAgreementType.AB,
                lon,
                Percentage.FullTime,
                startdatum);

            db.Employees.Add(employee);
        }

        await db.SaveChangesAsync();
    }
}
