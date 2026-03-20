using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Competence.Domain;

namespace RegionHR.Api.Endpoints;

public static class TalentEndpoints
{
    public static WebApplication MapTalentEndpoints(this WebApplication app)
    {
        var talent = app.MapGroup("/api/v1/talent").WithTags("Talent Marketplace").RequireAuthorization();

        // ============================================================
        // Lista karriärvägar
        // ============================================================
        talent.MapGet("/karriarvagar", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var paths = await db.CareerPaths
                .Include(cp => cp.Steg)
                .OrderBy(cp => cp.Namn)
                .ToListAsync(ct);

            return Results.Ok(paths.Select(cp => new
            {
                Id = cp.Id.Value,
                cp.Namn,
                cp.Bransch,
                cp.Beskrivning,
                Steg = cp.Steg.OrderBy(s => s.Ordning).Select(s => new
                {
                    s.Id,
                    s.Ordning,
                    s.Befattning,
                    s.TypiskTidManader,
                    s.KravdaSkills,
                    s.KravdErfarenhetManader
                })
            }));
        }).WithName("ListCareerPaths");

        // ============================================================
        // Lista interna möjligheter
        // ============================================================
        talent.MapGet("/mojligheter", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var opportunities = await db.InternalOpportunities
                .Where(o => o.Status == OpportunityStatus.Published)
                .OrderByDescending(o => o.PeriodFran)
                .ToListAsync(ct);

            return Results.Ok(opportunities.Select(o => new
            {
                Id = o.Id.Value,
                o.Typ,
                o.Titel,
                o.EnhetId,
                o.PeriodFran,
                o.PeriodTill,
                o.Kravprofil,
                Status = o.Status.ToString()
            }));
        }).WithName("ListOpportunities");

        // ============================================================
        // Ansök till intern möjlighet
        // ============================================================
        talent.MapPost("/mojligheter/{id}/ansok", async (Guid id, ApplyRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var opportunityId = SharedKernel.Domain.InternalOpportunityId.From(id);
            var opportunity = await db.InternalOpportunities
                .FirstOrDefaultAsync(o => o.Id == opportunityId, ct);

            if (opportunity is null)
                return Results.NotFound(new { error = "Möjlighet hittades inte" });

            if (opportunity.Status != OpportunityStatus.Published)
                return Results.BadRequest(new { error = "Möjligheten är inte öppen för ansökningar" });

            // Calculate match score based on employee skills vs requirements
            var matchScore = await CalculateMatchScore(db, req.AnstallId, opportunity.Kravprofil, ct);

            var application = OpportunityApplication.Skapa(opportunityId, req.AnstallId, req.Motivering, matchScore);
            await db.OpportunityApplications.AddAsync(application, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/talent/mojligheter/{id}/ansokningar/{application.Id}", new
            {
                application.Id,
                InternalOpportunityId = id,
                application.AnstallId,
                application.MatchScore,
                Status = application.Status.ToString()
            });
        }).WithName("ApplyToOpportunity");

        // ============================================================
        // Readiness-score per karriärväg för en anställd
        // ============================================================
        talent.MapGet("/readiness/{anstallId}", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var employeeSkills = await db.EmployeeSkills
                .Where(es => es.AnstallId == anstallId)
                .ToListAsync(ct);

            var allSkills = await db.Skills.ToListAsync(ct);
            var careerPaths = await db.CareerPaths
                .Include(cp => cp.Steg)
                .ToListAsync(ct);

            var results = careerPaths.Select(cp =>
            {
                var steg = cp.Steg.OrderBy(s => s.Ordning).ToList();
                var stepScores = steg.Select(s =>
                {
                    var score = CalculateStepReadiness(s, employeeSkills, allSkills);
                    return new
                    {
                        s.Befattning,
                        s.Ordning,
                        ReadinessScore = score
                    };
                }).ToList();

                var overallScore = stepScores.Count > 0
                    ? (int)stepScores.Average(s => s.ReadinessScore)
                    : 0;

                return new
                {
                    CareerPathId = cp.Id.Value,
                    cp.Namn,
                    cp.Bransch,
                    OverallReadiness = overallScore,
                    Steps = stepScores
                };
            });

            return Results.Ok(results);
        }).WithName("GetReadinessScores");

        return app;
    }

    private static async Task<int> CalculateMatchScore(RegionHRDbContext db, Guid anstallId,
        string? kravprofil, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(kravprofil)) return 50;

        var empSkills = await db.EmployeeSkills
            .Where(es => es.AnstallId == anstallId)
            .ToListAsync(ct);

        var allSkills = await db.Skills.ToListAsync(ct);

        // Simple match: count how many required skills the employee has
        try
        {
            var required = System.Text.Json.JsonSerializer.Deserialize<List<SkillReq>>(kravprofil) ?? [];
            if (required.Count == 0) return 50;

            int matched = 0;
            foreach (var req in required)
            {
                var skill = allSkills.FirstOrDefault(s => s.Namn == req.Skill);
                if (skill == null) continue;

                var empSkill = empSkills.FirstOrDefault(es => es.SkillId == skill.Id);
                if (empSkill != null && empSkill.Niva >= req.Niva)
                    matched++;
            }

            return (int)((double)matched / required.Count * 100);
        }
        catch
        {
            return 50;
        }
    }

    private static int CalculateStepReadiness(CareerPathStep step,
        List<EmployeeSkill> employeeSkills, List<Skill> allSkills)
    {
        if (string.IsNullOrWhiteSpace(step.KravdaSkills)) return 100;

        try
        {
            var required = System.Text.Json.JsonSerializer.Deserialize<List<SkillReq>>(step.KravdaSkills) ?? [];
            if (required.Count == 0) return 100;

            double totalScore = 0;
            foreach (var req in required)
            {
                var skill = allSkills.FirstOrDefault(s => s.Namn == req.Skill);
                if (skill == null) continue;

                var empSkill = employeeSkills.FirstOrDefault(es => es.SkillId == skill.Id);
                if (empSkill != null)
                {
                    var ratio = Math.Min((double)empSkill.Niva / req.Niva, 1.0);
                    totalScore += ratio * 100;
                }
            }

            return (int)(totalScore / required.Count);
        }
        catch
        {
            return 0;
        }
    }

    private record SkillReq(string Skill, int Niva);
}

record ApplyRequest(Guid AnstallId, string? Motivering);
