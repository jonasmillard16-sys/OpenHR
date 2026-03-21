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

        // ============================================================
        // F-skatt status
        // ============================================================

        vms.MapGet("/fskatt", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var registrations = await db.FSkattRegistrations
                .OrderByDescending(r => r.KontrolleradVid)
                .ToListAsync(ct);

            var vendors = await db.Vendors.ToListAsync(ct);
            var vendorMap = vendors.ToDictionary(v => v.Id, v => v.Namn);

            return Results.Ok(registrations.Select(r => new
            {
                r.Id,
                vendorId = r.VendorId?.Value,
                r.ContingentWorkerId,
                r.Organisationsnummer,
                fskattStatus = r.FSkattStatus.ToString(),
                r.KontrolleradVid,
                r.GiltigTill,
                kräverSkatteavdrag = r.KräverSkatteavdrag,
                snartUtgående = r.SnartUtgående(30),
                leverantörsnamn = r.VendorId.HasValue && vendorMap.TryGetValue(r.VendorId.Value, out var namn) ? namn : null
            }));
        }).WithName("ListFSkattRegistrations");

        vms.MapPost("/fskatt", async (CreateFSkattRegistrationDto dto, RegionHRDbContext db, CancellationToken ct) =>
        {
            var reg = FSkattRegistration.Skapa(
                dto.Organisationsnummer,
                Enum.Parse<FSkattStatus>(dto.Status),
                dto.GiltigTill,
                dto.ContingentWorkerId,
                dto.VendorId.HasValue ? VendorId.From(dto.VendorId.Value) : null);

            db.FSkattRegistrations.Add(reg);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/vms/fskatt/{reg.Id}", new
            {
                reg.Id,
                reg.Organisationsnummer,
                fskattStatus = reg.FSkattStatus.ToString(),
                reg.GiltigTill,
                kräverSkatteavdrag = reg.KräverSkatteavdrag
            });
        }).WithName("CreateFSkattRegistration");

        // ============================================================
        // Contractor classification
        // ============================================================

        vms.MapPost("/klassificering", async (CreateClassificationDto dto, RegionHRDbContext db, CancellationToken ct) =>
        {
            var classification = ContractorClassification.Bedöm(
                dto.ContingentWorkerId,
                dto.BedömningsResultat,
                dto.RiskNivå,
                dto.Faktorer,
                dto.BedömdAv);

            db.ContractorClassifications.Add(classification);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/vms/klassificering/{classification.Id}", new
            {
                classification.Id,
                classification.ContingentWorkerId,
                classification.BedömningsResultat,
                classification.RiskNivå,
                classification.Faktorer,
                classification.BedömdAv,
                classification.BedömdVid
            });
        }).WithName("CreateContractorClassification");

        vms.MapGet("/klassificeringar", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var classifications = await db.ContractorClassifications
                .OrderByDescending(c => c.BedömdVid)
                .ToListAsync(ct);

            return Results.Ok(classifications.Select(c => new
            {
                c.Id,
                c.ContingentWorkerId,
                c.BedömningsResultat,
                c.RiskNivå,
                c.Faktorer,
                c.BedömdAv,
                c.BedömdVid
            }));
        }).WithName("ListContractorClassifications");

        return app;
    }
}

// Request DTOs
record CreateFSkattRegistrationDto(
    string Organisationsnummer,
    string Status,
    DateOnly? GiltigTill,
    Guid? ContingentWorkerId,
    Guid? VendorId);

record CreateClassificationDto(
    Guid ContingentWorkerId,
    string BedömningsResultat,
    string RiskNivå,
    string Faktorer,
    string BedömdAv);

record CreateStaffingRequestDto(
    Guid EnhetId,
    string Befattning,
    DateOnly PeriodFran,
    DateOnly PeriodTill,
    int AntalPersoner,
    string Kravprofil);
