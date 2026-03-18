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
        // Personnummer with valid Luhn check digits
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

        foreach (var (fornamn, efternamn, pnr, befattning, enhetId, lon) in seedEmployees)
        {
            var employee = Employee.Skapa(
                Personnummer.CreateValidated(pnr),
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
