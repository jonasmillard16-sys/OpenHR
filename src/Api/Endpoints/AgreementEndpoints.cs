using Microsoft.EntityFrameworkCore;
using RegionHR.Agreements.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class AgreementEndpoints
{
    public static WebApplication MapAgreementEndpoints(this WebApplication app)
    {
        var avtal = app.MapGroup("/api/v1/avtal").WithTags("Kollektivavtal").RequireAuthorization();

        // ============================================================
        // Lista avtal
        // ============================================================

        avtal.MapGet("/", async (string? bransch, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.CollectiveAgreements.AsQueryable();

            if (!string.IsNullOrWhiteSpace(bransch) && Enum.TryParse<IndustrySector>(bransch, true, out var sector))
                query = query.Where(a => a.Bransch == sector);

            var agreements = await query
                .OrderBy(a => a.Namn)
                .ToListAsync(ct);

            return Results.Ok(agreements.Select(a => new
            {
                id = a.Id.Value,
                a.Namn,
                a.Parter,
                a.GiltigFran,
                a.GiltigTill,
                bransch = a.Bransch.ToString(),
                status = a.Status.ToString()
            }));
        }).WithName("ListAgreements");

        // ============================================================
        // Avtalsdetalj med alla underentiteter
        // ============================================================

        avtal.MapGet("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var agreement = await db.CollectiveAgreements
                .Include(a => a.OBSatser)
                .Include(a => a.OvertidsRegler)
                .Include(a => a.SemesterRegler)
                .Include(a => a.PensionsRegler)
                .Include(a => a.ViloRegler)
                .Include(a => a.ArbetstidsRegler)
                .Include("UppságningsRegler")
                .Include(a => a.ForsakringsRegler)
                .Include(a => a.LonestrukturRegler)
                .Include(a => a.PrivatErsattningsPlaner)
                .FirstOrDefaultAsync(a => a.Id == CollectiveAgreementId.From(id), ct);

            if (agreement is null)
                return Results.NotFound(new { error = "Avtal hittades inte" });

            return Results.Ok(new
            {
                id = agreement.Id.Value,
                agreement.Namn,
                agreement.Parter,
                agreement.GiltigFran,
                agreement.GiltigTill,
                bransch = agreement.Bransch.ToString(),
                status = agreement.Status.ToString(),
                obSatser = agreement.OBSatser.Select(ob => new
                {
                    ob.Id,
                    tidstyp = ob.Tidstyp.ToString(),
                    ob.Belopp,
                    ob.GiltigFran,
                    ob.GiltigTill
                }),
                overtidsRegler = agreement.OvertidsRegler.Select(ot => new
                {
                    ot.Id,
                    ot.Troskel,
                    ot.Multiplikator,
                    ot.MaxPerAr
                }),
                semesterRegler = agreement.SemesterRegler.Select(sr => new
                {
                    sr.Id,
                    sr.BasDagar,
                    sr.ExtraDagarVid40,
                    sr.ExtraDagarVid50
                }),
                pensionsRegler = agreement.PensionsRegler.Select(pr => new
                {
                    pr.Id,
                    pensionsTyp = pr.PensionsTyp.ToString(),
                    pr.SatsUnderTak,
                    pr.SatsOverTak,
                    pr.Tak,
                    pr.BerakningsModell
                }),
                viloRegler = agreement.ViloRegler.Select(vr => new
                {
                    vr.Id,
                    vr.MinDygnsvila,
                    vr.MinVeckovila,
                    vr.RastPerPass
                }),
                arbetstidsRegler = agreement.ArbetstidsRegler.Select(ar => new
                {
                    ar.Id,
                    ar.NormalTimmarPerVecka,
                    ar.FlexRegler
                }),
                forsakringsRegler = agreement.ForsakringsRegler.Select(fr => new
                {
                    fr.Id,
                    fr.TGL,
                    fr.AGS,
                    fr.TFA,
                    fr.AFA,
                    fr.PSA
                }),
                lonestrukturRegler = agreement.LonestrukturRegler.Select(ls => new
                {
                    ls.Id,
                    ls.MinLonPerKategori,
                    ls.LoneSteg
                })
            });
        }).WithName("GetAgreement");

        // ============================================================
        // Uppdatera OB-satser
        // ============================================================

        avtal.MapPut("/{id:guid}/ob-satser", async (Guid id, UpdateOBRatesRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var agreement = await db.CollectiveAgreements
                .Include(a => a.OBSatser)
                .FirstOrDefaultAsync(a => a.Id == CollectiveAgreementId.From(id), ct);

            if (agreement is null)
                return Results.NotFound(new { error = "Avtal hittades inte" });

            foreach (var obUpdate in req.OBSatser)
            {
                if (!Enum.TryParse<OBCategory>(obUpdate.Tidstyp, true, out var tidstyp))
                    return Results.BadRequest(new { error = $"Ogiltig tidstyp: {obUpdate.Tidstyp}" });

                agreement.LaggTillOBSats(tidstyp, obUpdate.Belopp, obUpdate.GiltigFran, obUpdate.GiltigTill);
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                id = agreement.Id.Value,
                obSatser = agreement.OBSatser.Select(ob => new
                {
                    ob.Id,
                    tidstyp = ob.Tidstyp.ToString(),
                    ob.Belopp,
                    ob.GiltigFran,
                    ob.GiltigTill
                })
            });
        }).WithName("UpdateAgreementOBRates");

        return app;
    }
}

// Request DTOs
record UpdateOBRatesRequest(List<OBRateDto> OBSatser);
record OBRateDto(string Tidstyp, decimal Belopp, DateOnly GiltigFran, DateOnly? GiltigTill);
