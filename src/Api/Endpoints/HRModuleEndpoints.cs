using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SalaryReview.Domain;
using RegionHR.Travel.Domain;
using RegionHR.Recruitment.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class HRModuleEndpoints
{
    public static WebApplication MapHRModuleEndpoints(this WebApplication app)
    {
        MapSalaryReviewEndpoints(app);
        MapTravelEndpoints(app);
        MapRecruitmentEndpoints(app);
        return app;
    }

    private static void MapSalaryReviewEndpoints(WebApplication app)
    {
        var lon = app.MapGroup("/api/v1/loneoversyn").WithTags("Löneöversyn").RequireAuthorization("ChefEllerHR");

        lon.MapGet("/rundor", async (int? ar, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.SalaryReviewRounds.AsQueryable();
            if (ar.HasValue) query = query.Where(r => r.Ar == ar.Value);

            var rundor = await query.OrderByDescending(r => r.Ar).Take(20).ToListAsync(ct);
            return Results.Ok(rundor.Select(r => new
            {
                r.Id, r.Namn, r.Ar, Avtal = r.Avtalsomrade.ToString(),
                Status = r.Status.ToString(), TotalBudget = r.TotalBudget.Amount,
                FordeladBudget = r.FordeladBudget.Amount, AterstaendeBudget = r.AterstaendeBudget.Amount,
                AntalForslag = r.Forslag.Count
            }));
        }).WithName("ListSalaryReviewRounds");

        lon.MapGet("/runda/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var runda = await db.SalaryReviewRounds.FirstOrDefaultAsync(r => r.Id == id, ct);
            return runda is not null ? Results.Ok(runda) : Results.NotFound();
        }).WithName("GetSalaryReviewRound");

        lon.MapPost("/runda", async (CreateSalaryReviewRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var runda = SalaryReviewRound.Skapa(req.Namn, req.Ar, req.Avtalsomrade,
                Money.SEK(req.Budget), req.IkrafttradandeDatum);
            await db.SalaryReviewRounds.AddAsync(runda, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/loneoversyn/runda/{runda.Id}", new { runda.Id });
        }).WithName("CreateSalaryReviewRound");

        lon.MapPost("/runda/{id:guid}/forslag", async (Guid id, AddProposalRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var runda = await db.SalaryReviewRounds.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (runda is null) return Results.NotFound();

            try
            {
                var forslag = runda.LaggTillForslag(
                    EmployeeId.From(req.AnstallId), Money.SEK(req.NuvarandeLon),
                    Money.SEK(req.ForeslagenLon), req.Motivering);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new
                {
                    forslag.Id, Okning = forslag.Okning.Amount,
                    forslag.OkningProcent, AterstaendeBudget = runda.AterstaendeBudget.Amount
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("AddSalaryProposal");

        lon.MapPost("/runda/{id:guid}/genomfor", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var runda = await db.SalaryReviewRounds.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (runda is null) return Results.NotFound();
            try
            {
                runda.Genomfor();
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { runda.Id, Status = runda.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("ExecuteSalaryReview");
    }

    private static void MapTravelEndpoints(WebApplication app)
    {
        var resa = app.MapGroup("/api/v1/resor").WithTags("Resor & Utlägg").RequireAuthorization();

        resa.MapGet("/", async (Guid? anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.TravelClaims.AsQueryable();
            if (anstallId.HasValue)
                query = query.Where(t => t.AnstallId == EmployeeId.From(anstallId.Value));

            var claims = await query.OrderByDescending(t => t.ReseDatum).Take(50).ToListAsync(ct);
            return Results.Ok(claims.Select(t => new
            {
                t.Id, t.AnstallId, t.Beskrivning, t.ReseDatum,
                Status = t.Status.ToString(), TotalBelopp = t.TotalBelopp.Amount
            }));
        }).WithName("ListTravelClaims");

        resa.MapPost("/", async (CreateTravelRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var claim = TravelClaim.Skapa(EmployeeId.From(req.AnstallId), req.Beskrivning, req.Datum);

            if (req.HelaDagar.HasValue || req.HalvaDagar.HasValue)
                claim.SattTraktamente(req.HelaDagar ?? 0, req.HalvaDagar ?? 0);
            if (req.KordaMil.HasValue)
                claim.SattMilersattning(req.KordaMil.Value);

            await db.TravelClaims.AddAsync(claim, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/resor/{claim.Id}", new { claim.Id, TotalBelopp = claim.TotalBelopp.Amount });
        }).WithName("CreateTravelClaim");

        resa.MapPost("/{id:guid}/utlagg", async (Guid id, AddExpenseRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var claim = await db.TravelClaims.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (claim is null) return Results.NotFound();
            claim.LaggTillUtlagg(req.Beskrivning, Money.SEK(req.Belopp), req.KvittoId);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { claim.Id, TotalBelopp = claim.TotalBelopp.Amount });
        }).WithName("AddExpense");

        resa.MapPost("/{id:guid}/skickain", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var claim = await db.TravelClaims.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (claim is null) return Results.NotFound();
            claim.SkickaIn();
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { claim.Id, Status = claim.Status.ToString() });
        }).WithName("SubmitTravelClaim");
    }

    private static void MapRecruitmentEndpoints(WebApplication app)
    {
        var rekr = app.MapGroup("/api/v1/rekrytering").WithTags("Rekrytering").RequireAuthorization("ChefEllerHR");

        rekr.MapGet("/vakanser", async (string? status, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.Vacancies.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<VacancyStatus>(status, true, out var s))
                query = query.Where(v => v.Status == s);

            var vakanser = await query.OrderByDescending(v => v.SistaAnsokningsDag).Take(50).ToListAsync(ct);
            return Results.Ok(vakanser.Select(v => new
            {
                v.Id, v.Titel, v.EnhetId, Status = v.Status.ToString(),
                v.SistaAnsokningsDag, v.PubliceradExternt, v.PubliceradPlatsbanken,
                AntalAnsokngar = v.Ansokngar.Count
            }));
        }).WithName("ListVacancies");

        rekr.MapGet("/vakans/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var vakans = await db.Vacancies.FirstOrDefaultAsync(v => v.Id == id, ct);
            return vakans is not null ? Results.Ok(vakans) : Results.NotFound();
        }).WithName("GetVacancy");

        rekr.MapPost("/vakans", async (CreateVacancyRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var vakans = Vacancy.Skapa(OrganizationId.From(req.EnhetId), req.Titel,
                req.Beskrivning, req.Anstallningsform, req.SistaDag);
            await db.Vacancies.AddAsync(vakans, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/rekrytering/vakans/{vakans.Id}", new { vakans.Id });
        }).WithName("CreateVacancy");

        rekr.MapPost("/vakans/{id:guid}/publicera", async (Guid id, PublishRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var vakans = await db.Vacancies.FirstOrDefaultAsync(v => v.Id == id, ct);
            if (vakans is null) return Results.NotFound();
            vakans.Publicera(req.Externt, req.Platsbanken);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { vakans.Id, Status = vakans.Status.ToString() });
        }).WithName("PublishVacancy");

        rekr.MapPost("/vakans/{id:guid}/ansok", async (Guid id, ApplicationRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var vakans = await db.Vacancies.FirstOrDefaultAsync(v => v.Id == id, ct);
            if (vakans is null) return Results.NotFound();
            try
            {
                var application = vakans.TaEmotAnsokan(req.Namn, req.Epost, req.CVFilId);
                await db.SaveChangesAsync(ct);
                return Results.Created($"/api/v1/rekrytering/vakans/{id}/ansokan/{application.Id}",
                    new { application.Id, application.Namn, Status = application.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("SubmitApplication");

        // ============================================================
        // Communication Templates
        // ============================================================

        rekr.MapGet("/mallar", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var mallar = await db.CommunicationTemplates
                .OrderByDescending(m => m.SkapadVid).ToListAsync(ct);
            return Results.Ok(mallar.Select(m => new
            {
                m.Id, m.Namn, Typ = m.Typ.ToString(), m.Amne, m.Brodtext, m.SkapadVid
            }));
        }).WithName("ListCommunicationTemplates");

        rekr.MapPost("/mall", async (CreateTemplateRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<TemplateType>(req.Typ, true, out var typ))
                return Results.BadRequest(new { error = $"Ogiltig malltyp: {req.Typ}. Giltiga: {string.Join(", ", Enum.GetNames<TemplateType>())}" });

            var mall = CommunicationTemplate.Skapa(req.Namn, typ, req.Amne, req.Brodtext);
            await db.CommunicationTemplates.AddAsync(mall, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/rekrytering/mallar", new
            {
                mall.Id, mall.Namn, Typ = mall.Typ.ToString(), mall.Amne
            });
        }).WithName("CreateCommunicationTemplate");

        // ============================================================
        // Onboarding Checklists
        // ============================================================

        rekr.MapGet("/onboarding", async (Guid? anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.OnboardingChecklists.AsQueryable();
            if (anstallId.HasValue)
                query = query.Where(o => o.AnstallId == anstallId.Value);

            var checklists = await query.OrderByDescending(o => o.SkapadVid).ToListAsync(ct);
            return Results.Ok(checklists.Select(o => new
            {
                o.Id, o.AnstallId, o.VakansId, o.Startdatum, o.AllaKlara, o.SkapadVid,
                AntalItems = o.Items.Count,
                AntalKlara = o.Items.Count(i => i.Klar),
                Items = o.Items.Select(i => new { i.Id, i.Beskrivning, i.Klar, i.KlarVid })
            }));
        }).WithName("ListOnboardingChecklists");

        rekr.MapPost("/onboarding", async (CreateOnboardingRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var checklist = OnboardingChecklist.Skapa(req.AnstallId, req.VakansId, req.Startdatum);
            await db.OnboardingChecklists.AddAsync(checklist, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/rekrytering/onboarding?anstallId={req.AnstallId}", new
            {
                checklist.Id, checklist.AnstallId, checklist.VakansId,
                AntalItems = checklist.Items.Count
            });
        }).WithName("CreateOnboardingChecklist");

        rekr.MapPost("/onboarding/{id:guid}/klara/{index:int}", async (Guid id, int index, RegionHRDbContext db, CancellationToken ct) =>
        {
            var checklist = await db.OnboardingChecklists.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (checklist is null) return Results.NotFound();

            try
            {
                checklist.MarkeraKlar(index);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new
                {
                    checklist.Id, checklist.AllaKlara,
                    MarkeradItem = checklist.Items[index].Beskrivning,
                    AntalKlara = checklist.Items.Count(i => i.Klar),
                    AntalTotalt = checklist.Items.Count
                });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("MarkOnboardingItemComplete");

        // ============================================================
        // Requisition Approvals (Godkännande av tjänstetillsättning)
        // ============================================================

        rekr.MapPost("/vakans/{id:guid}/godkannande", async (Guid id, CreateRequisitionApprovalRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var vakansExists = await db.Vacancies.AnyAsync(v => v.Id == id, ct);
            if (!vakansExists) return Results.NotFound();

            var approval = RequisitionApproval.Skapa(id, req.GodkannareId);
            await db.RequisitionApprovals.AddAsync(approval, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/rekrytering/godkannande/{approval.Id}", new
            {
                approval.Id, approval.VakansId, approval.GodkannareId,
                Status = approval.Status.ToString()
            });
        }).WithName("CreateRequisitionApproval");

        rekr.MapPost("/godkannande/{id:guid}/godkann", async (Guid id, ApproveRequisitionRequest? req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var approval = await db.RequisitionApprovals.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (approval is null) return Results.NotFound();

            approval.Godkann(req?.Kommentar);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new
            {
                approval.Id, Status = approval.Status.ToString(), approval.BeslutVid, approval.Kommentar
            });
        }).WithName("ApproveRequisition");

        rekr.MapPost("/godkannande/{id:guid}/neka", async (Guid id, RejectRequisitionRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var approval = await db.RequisitionApprovals.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (approval is null) return Results.NotFound();

            approval.Neka(req.Kommentar);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new
            {
                approval.Id, Status = approval.Status.ToString(), approval.BeslutVid, approval.Kommentar
            });
        }).WithName("RejectRequisition");

        // ============================================================
        // Interview Scheduling (Intervjubokning)
        // ============================================================

        rekr.MapPost("/intervju", async (CreateInterviewRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var intervju = InterviewSchedule.Skapa(req.ApplicationId, req.Tidpunkt, req.LangdMinuter, req.Plats, req.InterviewerIds);
            await db.InterviewSchedules.AddAsync(intervju, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/rekrytering/intervju/{intervju.Id}", new
            {
                intervju.Id, intervju.ApplicationId, intervju.Tidpunkt,
                intervju.LangdMinuter, intervju.Plats
            });
        }).WithName("ScheduleInterview");

        rekr.MapPost("/intervju/{id:guid}/genomford", async (Guid id, MarkInterviewDoneRequest? req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var intervju = await db.InterviewSchedules.FirstOrDefaultAsync(i => i.Id == id, ct);
            if (intervju is null) return Results.NotFound();

            intervju.MarkeraSomGenomford(req?.Anteckningar);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new
            {
                intervju.Id, intervju.Genomford, intervju.Anteckningar
            });
        }).WithName("MarkInterviewDone");

        // ============================================================
        // Scorecards (Bedömningskort)
        // ============================================================

        rekr.MapPost("/scorecard", async (CreateScorecardRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            try
            {
                var scorecard = Scorecard.Skapa(req.ApplicationId, req.BedomareId,
                    req.KompetensPoang, req.ErfarenhetsPoang, req.PersonlighetPoang, req.MotivationPoang,
                    req.Kommentar, req.Rekommendation);
                await db.Scorecards.AddAsync(scorecard, ct);
                await db.SaveChangesAsync(ct);
                return Results.Created($"/api/v1/rekrytering/scorecard/{scorecard.Id}", new
                {
                    scorecard.Id, scorecard.ApplicationId, scorecard.BedomareId,
                    scorecard.TotalPoang, scorecard.Rekommendation
                });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("CreateScorecard");

        rekr.MapGet("/vakans/{id:guid}/scorecards", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var vakans = await db.Vacancies.FirstOrDefaultAsync(v => v.Id == id, ct);
            if (vakans is null) return Results.NotFound();

            var applicationIds = vakans.Ansokngar.Select(a => a.Id).ToList();
            var scorecards = await db.Scorecards
                .Where(s => applicationIds.Contains(s.ApplicationId))
                .OrderByDescending(s => s.SkapadVid)
                .ToListAsync(ct);
            return Results.Ok(scorecards.Select(s => new
            {
                s.Id, s.ApplicationId, s.BedomareId,
                s.KompetensPoang, s.ErfarenhetsPoang, s.PersonlighetPoang, s.MotivationPoang,
                s.TotalPoang, s.Kommentar, s.Rekommendation, s.SkapadVid
            }));
        }).WithName("ListScorecardsForVacancy");

        // ============================================================
        // Talent Pool
        // ============================================================

        rekr.MapGet("/talentpool", async (string? kompetens, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.TalentPoolEntries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(kompetens))
                query = query.Where(t => t.KompetensOmrade != null && t.KompetensOmrade.ToLower().Contains(kompetens.ToLower()));

            var entries = await query.OrderByDescending(t => t.SkapadVid).Take(100).ToListAsync(ct);
            return Results.Ok(entries.Select(t => new
            {
                t.Id, t.Namn, t.Epost, t.Telefon, t.KompetensOmrade,
                t.Anteckningar, t.UrsprungsAnsokanId, t.SkapadVid
            }));
        }).WithName("ListTalentPool");

        rekr.MapPost("/talentpool", async (CreateTalentPoolRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var entry = TalentPoolEntry.Skapa(req.Namn, req.Epost, req.KompetensOmrade, req.Anteckningar, req.UrsprungsAnsokanId);
            await db.TalentPoolEntries.AddAsync(entry, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/rekrytering/talentpool", new
            {
                entry.Id, entry.Namn, entry.Epost, entry.KompetensOmrade
            });
        }).WithName("AddToTalentPool");
    }
}

// Request DTOs
record CreateSalaryReviewRequest(string Namn, int Ar, CollectiveAgreementType Avtalsomrade, decimal Budget, DateOnly IkrafttradandeDatum);
record AddProposalRequest(Guid AnstallId, decimal NuvarandeLon, decimal ForeslagenLon, string Motivering);
record CreateTravelRequest(Guid AnstallId, string Beskrivning, DateOnly Datum, int? HelaDagar = null, int? HalvaDagar = null, decimal? KordaMil = null);
record AddExpenseRequest(string Beskrivning, decimal Belopp, string? KvittoId = null);
record CreateVacancyRequest(Guid EnhetId, string Titel, string Beskrivning, EmploymentType Anstallningsform, DateOnly SistaDag);
record PublishRequest(bool Externt = true, bool Platsbanken = false);
record ApplicationRequest(string Namn, string Epost, string? CVFilId = null);
record CreateTemplateRequest(string Namn, string Typ, string Amne, string Brodtext);
record CreateOnboardingRequest(Guid AnstallId, Guid VakansId, DateOnly Startdatum);
record CreateRequisitionApprovalRequest(Guid GodkannareId);
record ApproveRequisitionRequest(string? Kommentar = null);
record RejectRequisitionRequest(string Kommentar);
record CreateInterviewRequest(Guid ApplicationId, DateTime Tidpunkt, int LangdMinuter, string Plats, List<Guid>? InterviewerIds = null);
record MarkInterviewDoneRequest(string? Anteckningar = null);
record CreateScorecardRequest(Guid ApplicationId, Guid BedomareId, int KompetensPoang, int ErfarenhetsPoang, int PersonlighetPoang, int MotivationPoang, string? Kommentar = null, string? Rekommendation = null);
record CreateTalentPoolRequest(string Namn, string Epost, string? KompetensOmrade = null, string? Anteckningar = null, Guid? UrsprungsAnsokanId = null);
