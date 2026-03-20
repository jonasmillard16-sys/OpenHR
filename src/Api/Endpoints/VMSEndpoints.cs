using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;
using RegionHR.VMS.Domain;

namespace RegionHR.Api.Endpoints;

public static class VMSEndpoints
{
    public static WebApplication MapVMSEndpoints(this WebApplication app)
    {
        var vms = app.MapGroup("/api/v1/vms").WithTags("VMS").RequireAuthorization();

        // ============================================================
        // Leverantörer
        // ============================================================

        vms.MapGet("/leverantorer", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var vendors = await db.Vendors
                .OrderBy(v => v.Namn)
                .ToListAsync(ct);

            return Results.Ok(vendors.Select(v => new
            {
                id = v.Id.Value,
                v.Namn,
                v.OrgNummer,
                v.Kontaktperson,
                v.Epost,
                v.Telefon,
                v.Kategori,
                status = v.Status.ToString()
            }));
        }).WithName("ListVendors");

        // ============================================================
        // Ramavtal
        // ============================================================

        vms.MapGet("/ramavtal", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var agreements = await db.FrameworkAgreements
                .Include(a => a.RateCards)
                .OrderBy(a => a.GiltigFran)
                .ToListAsync(ct);

            return Results.Ok(agreements.Select(a => new
            {
                id = a.Id.Value,
                vendorId = a.VendorId.Value,
                a.GiltigFran,
                a.GiltigTill,
                a.Avtalsvillkor,
                a.UppságningstidManader,
                a.ForlangningsKlausul,
                a.Avtalsvarde,
                rateCards = a.RateCards.Select(rc => new
                {
                    rc.Id,
                    rc.YrkesKategori,
                    rc.TimPris,
                    rc.OBPaslag,
                    rc.OvertidPaslag,
                    rc.Moms
                })
            }));
        }).WithName("ListFrameworkAgreements");

        // ============================================================
        // Beställningar (skapa)
        // ============================================================

        vms.MapPost("/bestallningar", async (CreateStaffingRequestDto dto, RegionHRDbContext db, CancellationToken ct) =>
        {
            var request = StaffingRequest.Skapa(
                OrganizationId.From(dto.EnhetId),
                dto.Befattning,
                dto.PeriodFran,
                dto.PeriodTill,
                dto.AntalPersoner,
                dto.Kravprofil);

            db.StaffingRequests.Add(request);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/vms/bestallningar/{request.Id.Value}", new
            {
                id = request.Id.Value,
                enhetId = request.EnhetId.Value,
                request.Befattning,
                request.PeriodFran,
                request.PeriodTill,
                request.AntalPersoner,
                request.Kravprofil,
                status = request.Status.ToString()
            });
        }).WithName("CreateStaffingRequest");

        // ============================================================
        // Statistik
        // ============================================================

        vms.MapGet("/statistik", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var activeWorkers = await db.ContingentWorkers
                .Where(w => w.Slutdatum == null || w.Slutdatum >= DateOnly.FromDateTime(DateTime.Today))
                .ToListAsync(ct);

            var vendors = await db.Vendors.ToListAsync(ct);
            var invoices = await db.VendorInvoices.ToListAsync(ct);
            var categories = await db.SpendCategories.ToListAsync(ct);

            var totalKostnad = activeWorkers.Sum(w => w.TimKostnad * 160); // Uppskattad månadskostnad
            var totalFakturerat = invoices.Sum(i => i.Belopp);
            var avvikelser = invoices.Count(i => i.Differens.HasValue && Math.Abs(i.Differens.Value) > 0.01m);

            return Results.Ok(new
            {
                aktivaInhyrda = activeWorkers.Count,
                antalLeverantorer = vendors.Count(v => v.Status == VendorStatus.Active),
                totalKostnad,
                totalFakturerat,
                avvikelser,
                perLeverantor = vendors.Select(v => new
                {
                    leverantor = v.Namn,
                    antal = activeWorkers.Count(w => w.VendorId == v.Id),
                    kostnad = activeWorkers.Where(w => w.VendorId == v.Id).Sum(w => w.TimKostnad * 160)
                }).Where(x => x.antal > 0),
                spendKategorier = categories.Select(c => new
                {
                    kategori = c.Namn,
                    c.Beskrivning
                })
            });
        }).WithName("GetVMSStatistics");

        return app;
    }
}

// Request DTOs
record CreateStaffingRequestDto(
    Guid EnhetId,
    string Befattning,
    DateOnly PeriodFran,
    DateOnly PeriodTill,
    int AntalPersoner,
    string Kravprofil);
