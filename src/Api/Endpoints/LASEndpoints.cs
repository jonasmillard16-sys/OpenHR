using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.LAS.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class LASEndpoints
{
    public static WebApplication MapLASEndpoints(this WebApplication app)
    {
        var las = app.MapGroup("/api/v1/las").WithTags("LAS").RequireAuthorization("ChefEllerHR");

        las.MapGet("/ackumuleringar", async (string? status, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.LASAccumulations.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LASStatus>(status, true, out var s))
                query = query.Where(a => a.Status == s);

            var result = await query
                .OrderByDescending(a => a.AckumuleradeDagar)
                .Take(100)
                .ToListAsync(ct);

            return Results.Ok(result.Select(a => new
            {
                a.Id, a.AnstallId, Anstallningsform = a.Anstallningsform.ToString(),
                a.AckumuleradeDagar, Status = a.Status.ToString(),
                a.KonverteringsDatum, a.HarForetradesratt, a.ForetradesrattUtgar
            }));
        }).WithName("ListLASAccumulations");

        las.MapGet("/alarmeringar", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var alarmeringar = await db.LASAccumulations
                .Where(a => a.Status == LASStatus.NaraGrans || a.Status == LASStatus.KritiskNara)
                .OrderByDescending(a => a.AckumuleradeDagar)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                Antal = alarmeringar.Count,
                Alarmeringar = alarmeringar.Select(a => new
                {
                    a.AnstallId, Anstallningsform = a.Anstallningsform.ToString(),
                    a.AckumuleradeDagar, Status = a.Status.ToString()
                })
            });
        }).WithName("GetLASAlarms");

        las.MapGet("/foretradesratt", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var innehavare = await db.LASAccumulations
                .Where(a => a.HarForetradesratt && (a.ForetradesrattUtgar == null || a.ForetradesrattUtgar >= today))
                .ToListAsync(ct);

            return Results.Ok(innehavare.Select(a => new
            {
                a.AnstallId, a.AckumuleradeDagar, a.ForetradesrattUtgar
            }));
        }).WithName("GetPreferentialRights");

        las.MapGet("/dashboard", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var all = await db.LASAccumulations.ToListAsync(ct);
            return Results.Ok(new
            {
                TotaltAktiva = all.Count,
                UnderGrans = all.Count(a => a.Status == LASStatus.UnderGrans),
                NaraGrans = all.Count(a => a.Status == LASStatus.NaraGrans),
                KritiskNara = all.Count(a => a.Status == LASStatus.KritiskNara),
                Konverterade = all.Count(a => a.Status == LASStatus.KonverteradTillTillsvidare),
                MedForetradesratt = all.Count(a => a.HarForetradesratt),
                TopNarmast = all
                    .Where(a => a.Status != LASStatus.KonverteradTillTillsvidare)
                    .OrderByDescending(a => a.AckumuleradeDagar)
                    .Take(10)
                    .Select(a => new { a.AnstallId, a.AckumuleradeDagar, Status = a.Status.ToString() })
            });
        }).WithName("GetLASDashboard");

        return app;
    }
}
