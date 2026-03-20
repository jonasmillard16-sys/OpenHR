using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Compensation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class CompensationEndpoints
{
    public static WebApplication MapCompensationEndpoints(this WebApplication app)
    {
        var comp = app.MapGroup("/api/v1/compensation").WithTags("Compensation").RequireAuthorization("LonOchHR");

        // ============================================================
        // Kompensationsplaner
        // ============================================================

        comp.MapGet("/planer", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var planer = await db.CompensationPlans
                .OrderByDescending(p => p.GiltigFran)
                .Take(20)
                .ToListAsync(ct);

            return Results.Ok(planer.Select(p => new
            {
                p.Id, p.Namn, p.GiltigFran, p.GiltigTill,
                p.TotalBudget, Status = p.Status.ToString()
            }));
        }).WithName("ListCompensationPlans");

        // ============================================================
        // Kompensationsband
        // ============================================================

        comp.MapGet("/band", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var band = await db.CompensationBands
                .OrderBy(b => b.Befattningskategori)
                .ToListAsync(ct);

            return Results.Ok(band.Select(b => new
            {
                b.Id, b.Befattningskategori, b.Min, b.Mal, b.Max,
                b.Steg1Min, b.Steg1Max, b.Steg2Min, b.Steg2Max,
                b.Steg3Min, b.Steg3Max, b.Steg4Min, b.Steg4Max
            }));
        }).WithName("ListCompensationBands");

        // ============================================================
        // Simulering
        // ============================================================

        comp.MapPost("/simulering", async (SimuleringRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            // Hitta bandet for befattningen
            var band = await db.CompensationBands
                .FirstOrDefaultAsync(b => b.Befattningskategori == req.Befattning, ct);

            decimal nuvarandeLon = req.NuvarandeLon;
            decimal nyLon = nuvarandeLon * (1 + req.HojningProcent / 100m);

            // AG-avgift 31.42%
            const decimal agAvgiftProcent = 31.42m;
            decimal agAvgiftNuvarande = nuvarandeLon * agAvgiftProcent / 100m;
            decimal agAvgiftNy = nyLon * agAvgiftProcent / 100m;

            decimal totalKostnadNuvarande = nuvarandeLon + agAvgiftNuvarande;
            decimal totalKostnadNy = nyLon + agAvgiftNy;
            decimal okadKostnadPerManad = totalKostnadNy - totalKostnadNuvarande;
            decimal okadKostnadPerAr = okadKostnadPerManad * 12m;

            bool inomBand = band?.ArInomBand(nyLon) ?? true;
            decimal? bandPosition = band?.BandPosition(nyLon);

            // Spara simuleringen
            var sim = CompensationSimulation.Skapa(
                $"Simulering {req.Befattning} {req.HojningProcent}%",
                System.Text.Json.JsonSerializer.Serialize(req),
                "system");

            var resultat = new
            {
                NuvarandeLon = nuvarandeLon,
                NyLon = Math.Round(nyLon, 0),
                HojningKr = Math.Round(nyLon - nuvarandeLon, 0),
                HojningProcent = req.HojningProcent,
                AGAvgiftNuvarande = Math.Round(agAvgiftNuvarande, 0),
                AGAvgiftNy = Math.Round(agAvgiftNy, 0),
                TotalKostnadNuvarande = Math.Round(totalKostnadNuvarande, 0),
                TotalKostnadNy = Math.Round(totalKostnadNy, 0),
                OkadKostnadPerManad = Math.Round(okadKostnadPerManad, 0),
                OkadKostnadPerAr = Math.Round(okadKostnadPerAr, 0),
                InomBand = inomBand,
                BandPosition = bandPosition.HasValue ? Math.Round(bandPosition.Value, 1) : (decimal?)null,
                BandMin = band?.Min,
                BandMal = band?.Mal,
                BandMax = band?.Max
            };

            sim.SattResultat(System.Text.Json.JsonSerializer.Serialize(resultat));
            await db.CompensationSimulations.AddAsync(sim, ct);
            await db.SaveChangesAsync(ct);

            return Results.Ok(resultat);
        }).WithName("RunCompensationSimulation");

        // ============================================================
        // Total Rewards Statement
        // ============================================================

        comp.MapGet("/totalrewards/{anstallId:guid}", async (Guid anstallId, int? ar, RegionHRDbContext db, CancellationToken ct) =>
        {
            var year = ar ?? DateTime.Now.Year;
            var statement = await db.TotalRewardsStatements
                .FirstOrDefaultAsync(s => s.AnstallId == EmployeeId.From(anstallId) && s.Ar == year, ct);

            if (statement is null)
            {
                // Generera en ny baserat pa befintliga data
                var emp = await db.Employees
                    .Include(e => e.Anstallningar)
                    .FirstOrDefaultAsync(e => e.Id == EmployeeId.From(anstallId), ct);

                if (emp is null) return Results.NotFound();

                var aktivAnstallning = emp.Anstallningar.FirstOrDefault();
                if (aktivAnstallning is null) return Results.NotFound("Ingen aktiv anstallning");

                decimal grundLon = aktivAnstallning.Manadslon.Amount;
                decimal tillagg = grundLon * 0.05m; // Uppskattade tillagg
                decimal pension = grundLon * 0.045m; // KAP-KL
                decimal forsakringar = 800m;
                decimal formaner = 2500m; // Friskvard + subventionerad lunch
                decimal agAvgifter = grundLon * 0.3142m;

                statement = TotalRewardsStatement.Generera(
                    EmployeeId.From(anstallId), year,
                    grundLon, tillagg, pension, forsakringar, formaner, agAvgifter);

                await db.TotalRewardsStatements.AddAsync(statement, ct);
                await db.SaveChangesAsync(ct);
            }

            return Results.Ok(new
            {
                statement.Id,
                statement.AnstallId,
                statement.Ar,
                statement.GrundLon,
                statement.Tillagg,
                statement.Pension,
                statement.Forsakringar,
                statement.Formaner,
                statement.AGAvgifter,
                statement.TotalKompensation,
                statement.GenereradVid
            });
        }).WithName("GetTotalRewardsStatement");

        return app;
    }
}

record SimuleringRequest(string Befattning, decimal NuvarandeLon, decimal HojningProcent);
