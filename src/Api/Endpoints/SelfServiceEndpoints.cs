using Microsoft.EntityFrameworkCore;
using RegionHR.Core.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class SelfServiceEndpoints
{
    public static WebApplication MapSelfServiceEndpoints(this WebApplication app)
    {
        var ms = app.MapGroup("/api/v1/minsida").WithTags("Min Sida");

        // Dashboard - aggregated overview for an employee
        ms.MapGet("/dashboard/{anstallId:guid}", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var empId = EmployeeId.From(anstallId);
            var emp = await db.Employees.Include(e => e.Anstallningar)
                .FirstOrDefaultAsync(e => e.Id == empId, ct);
            if (emp is null) return Results.NotFound();

            // Vacation balance current year
            var year = DateTime.Today.Year;
            var vacBalance = await db.VacationBalances
                .FirstOrDefaultAsync(b => b.AnstallId == anstallId && b.Ar == year, ct);

            // Upcoming shifts (next 7 days)
            var today = DateOnly.FromDateTime(DateTime.Today);
            var weekEnd = today.AddDays(7);
            var shifts = await db.ScheduledShifts
                .Where(s => s.AnstallId == empId && s.Datum >= today && s.Datum <= weekEnd)
                .OrderBy(s => s.Datum)
                .Take(10)
                .ToListAsync(ct);

            // Open leave requests
            var leaveRequests = await db.LeaveRequests
                .Where(r => r.AnstallId == anstallId && (r.Status == Leave.Domain.LeaveRequestStatus.Utkast || r.Status == Leave.Domain.LeaveRequestStatus.Inskickad))
                .OrderByDescending(r => r.SkapadVid)
                .Take(5)
                .ToListAsync(ct);

            // Active cases
            var cases = await db.Cases
                .Where(c => c.AnstallId == empId && c.Status != CaseStatus.Avslutad)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync(ct);

            // Unread notifications
            var unreadNotifs = await db.Notifications
                .CountAsync(n => n.UserId == anstallId && !n.IsRead, ct);

            // Expiring certifications
            var deadline = today.AddDays(90);
            var expiringCerts = await db.Certifications
                .Where(c => c.AnstallId == anstallId && c.GiltigTill != null && c.GiltigTill <= deadline && c.GiltigTill >= today)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                anstalld = new { emp.Fornamn, emp.Efternamn, Personnummer = emp.Personnummer.ToMaskedString(), emp.Epost },
                semester = vacBalance != null ? new { vacBalance.Tilldelning, vacBalance.UttagnaDagar, vacBalance.TillgangligaDagar, vacBalance.SparadeDagar } : null,
                kommandePass = shifts.Select(s => new { s.Datum, PassTyp = s.PassTyp.ToString(), s.PlaneradStart, s.PlaneradSlut }),
                oppnaLedighetsansokningar = leaveRequests.Select(r => new { r.Id, Typ = r.Typ.ToString(), r.FranDatum, r.TillDatum, Status = r.Status.ToString() }),
                aktivaArenden = cases.Select(c => new { c.Id, Typ = c.Typ.ToString(), Status = c.Status.ToString(), c.Beskrivning }),
                olastNotiser = unreadNotifs,
                utgaendeCertifikat = expiringCerts.Select(c => new { c.Namn, c.GiltigTill })
            });
        }).WithName("GetMinSidaDashboard");

        // Salary history
        ms.MapGet("/lonhistorik/{anstallId:guid}", async (Guid anstallId, int? ar, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.PayrollResults.Where(r => r.AnstallId == EmployeeId.From(anstallId));
            if (ar.HasValue) query = query.Where(r => r.Year == ar.Value);

            var results = await query.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).Take(24).ToListAsync(ct);
            return Results.Ok(results.Select(r => new
            {
                r.Year, r.Month, Brutto = r.Brutto.Amount, Skatt = r.Skatt.Amount,
                Netto = r.Netto.Amount, OB = r.OBTillagg.Amount, Overtid = r.Overtidstillagg.Amount
            }));
        }).WithName("GetSalaryHistory");

        // ============================================================
        // Nödkontakter (Emergency Contacts)
        // ============================================================

        ms.MapGet("/noddkontakter/{anstallId:guid}", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var kontakter = await db.EmergencyContacts
                .Where(k => k.AnstallId == anstallId)
                .OrderByDescending(k => k.ArPrimar)
                .ToListAsync(ct);
            return Results.Ok(kontakter.Select(k => new
            {
                k.Id, k.AnstallId, k.Namn, k.Relation, k.Telefon, k.Epost, k.ArPrimar
            }));
        }).WithName("ListEmergencyContacts");

        ms.MapPost("/noddkontakt", async (CreateEmergencyContactRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var kontakt = EmergencyContact.Skapa(req.AnstallId, req.Namn, req.Relation, req.Telefon, req.Epost, req.ArPrimar);
            await db.EmergencyContacts.AddAsync(kontakt, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/minsida/noddkontakter/{req.AnstallId}", new
            {
                kontakt.Id, kontakt.AnstallId, kontakt.Namn, kontakt.Relation
            });
        }).WithName("CreateEmergencyContact");

        ms.MapPut("/noddkontakt/{id:guid}", async (Guid id, UpdateEmergencyContactRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var kontakt = await db.EmergencyContacts.FirstOrDefaultAsync(k => k.Id == id, ct);
            if (kontakt is null) return Results.NotFound();

            kontakt.Uppdatera(req.Namn, req.Relation, req.Telefon, req.Epost);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { kontakt.Id, kontakt.Namn, kontakt.Relation, kontakt.Telefon, kontakt.Epost });
        }).WithName("UpdateEmergencyContact");

        ms.MapDelete("/noddkontakt/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var kontakt = await db.EmergencyContacts.FirstOrDefaultAsync(k => k.Id == id, ct);
            if (kontakt is null) return Results.NotFound();

            db.EmergencyContacts.Remove(kontakt);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { deleted = true, kontakt.Id });
        }).WithName("DeleteEmergencyContact");

        // ============================================================
        // Uppdatera egna kontaktuppgifter
        // ============================================================

        ms.MapPut("/kontaktuppgifter/{anstallId:guid}", async (Guid anstallId, UpdateContactInfoRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var emp = await db.Employees.FirstOrDefaultAsync(e => e.Id == EmployeeId.From(anstallId), ct);
            if (emp is null) return Results.NotFound();

            Address? adress = req.Gatuadress != null ? new Address(req.Gatuadress, req.Postnummer ?? "", req.Ort ?? "") : null;
            emp.UppdateraKontaktuppgifter(req.Epost ?? emp.Epost, req.Telefon ?? emp.Telefon, adress ?? emp.Adress);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { emp.Id, emp.Epost, emp.Telefon, emp.Adress });
        }).WithName("UpdateOwnContactInfo");

        // ============================================================
        // Uppdatera egna bankuppgifter
        // ============================================================

        ms.MapPut("/bankuppgifter/{anstallId:guid}", async (Guid anstallId, UpdateBankDetailsRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var emp = await db.Employees.FirstOrDefaultAsync(e => e.Id == EmployeeId.From(anstallId), ct);
            if (emp is null) return Results.NotFound();

            emp.UppdateraBankuppgifter(req.Clearingnummer, req.Kontonummer);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { emp.Id, message = "Bankuppgifter uppdaterade" });
        }).WithName("UpdateOwnBankDetails");

        // ============================================================
        // Spåra egna ärenden
        // ============================================================

        ms.MapGet("/arenden/{anstallId:guid}", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var empId = EmployeeId.From(anstallId);
            var arenden = await db.Cases
                .Where(c => c.AnstallId == empId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)
                .ToListAsync(ct);
            return Results.Ok(arenden.Select(c => new
            {
                c.Id, Typ = c.Typ.ToString(), Status = c.Status.ToString(), c.Beskrivning, c.CreatedAt
            }));
        }).WithName("TrackOwnCases");

        return app;
    }
}

// Self-Service Request DTOs
record CreateEmergencyContactRequest(Guid AnstallId, string Namn, string Relation, string Telefon, string? Epost = null, bool ArPrimar = false);
record UpdateEmergencyContactRequest(string Namn, string Relation, string Telefon, string? Epost = null);
record UpdateContactInfoRequest(string? Epost = null, string? Telefon = null, string? Gatuadress = null, string? Postnummer = null, string? Ort = null);
record UpdateBankDetailsRequest(string Clearingnummer, string Kontonummer);
