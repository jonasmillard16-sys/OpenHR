using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.LMS.Domain;

namespace RegionHR.Api.Endpoints;

public static class LMSEndpoints
{
    public static WebApplication MapLMSEndpoints(this WebApplication app)
    {
        var utbildning = app.MapGroup("/api/v1/utbildning").WithTags("Utbildning / LMS").RequireAuthorization();

        // ============================================================
        // Lista kurser
        // ============================================================

        utbildning.MapGet("/kurser", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var courses = await db.Courses
                .OrderBy(c => c.Namn)
                .ToListAsync(ct);

            return Results.Ok(courses.Select(c => new
            {
                c.Id, c.Namn, c.Beskrivning,
                Format = c.Format.ToString(),
                Status = c.Status.ToString(),
                c.LangdMinuter, c.ArObligatorisk,
                c.Kategori, c.GiltighetManader, c.MaxDeltagare
            }));
        }).WithName("ListCourses");

        // ============================================================
        // Skapa kurs
        // ============================================================

        utbildning.MapPost("/kurs", async (CreateCourseRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<CourseFormat>(req.Format, true, out var format))
                return Results.BadRequest(new { error = $"Ogiltigt format: {req.Format}. Giltiga värden: {string.Join(", ", Enum.GetNames<CourseFormat>())}" });

            var course = Course.Skapa(req.Namn, req.Beskrivning, format, req.LangdMinuter, req.Obligatorisk, req.Kategori, req.GiltighetManader, req.MaxDeltagare);
            await db.Courses.AddAsync(course, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/utbildning/kurs/{course.Id}", new
            {
                course.Id, course.Namn,
                Format = course.Format.ToString(),
                Status = course.Status.ToString(),
                course.LangdMinuter, course.ArObligatorisk
            });
        }).WithName("CreateCourse");

        // ============================================================
        // Publicera kurs
        // ============================================================

        utbildning.MapPost("/kurs/{id:guid}/publicera", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (course is null) return Results.NotFound();

            course.Publicera();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { course.Id, Status = course.Status.ToString() });
        }).WithName("PublishCourse");

        // ============================================================
        // Lista kursanmälningar
        // ============================================================

        utbildning.MapGet("/anmalningar", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var enrollments = await db.CourseEnrollments
                .Where(e => e.AnstallId == anstallId)
                .OrderByDescending(e => e.AnmalanVid)
                .ToListAsync(ct);

            return Results.Ok(enrollments.Select(e => new
            {
                e.Id, e.AnstallId, e.CourseId,
                Progress = e.Progress.ToString(),
                e.Resultat, e.Godkand,
                e.AnmalanVid, e.PaborjadVid, e.GenomfordVid, e.GiltigTill
            }));
        }).WithName("ListCourseEnrollments");

        // ============================================================
        // Anmäl till kurs
        // ============================================================

        utbildning.MapPost("/anmalan", async (CreateCourseEnrollmentRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == req.CourseId, ct);
            if (course is null) return Results.NotFound(new { error = "Kurs hittades inte" });

            var enrollment = CourseEnrollment.Anmala(req.AnstallId, req.CourseId);
            await db.CourseEnrollments.AddAsync(enrollment, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/utbildning/anmalan/{enrollment.Id}", new
            {
                enrollment.Id, enrollment.AnstallId, enrollment.CourseId,
                Progress = enrollment.Progress.ToString()
            });
        }).WithName("EnrollInCourse");

        // ============================================================
        // Påbörja kurs
        // ============================================================

        utbildning.MapPost("/anmalan/{id:guid}/paborja", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var enrollment = await db.CourseEnrollments.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (enrollment is null) return Results.NotFound();

            enrollment.Paborja();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { enrollment.Id, Progress = enrollment.Progress.ToString(), enrollment.PaborjadVid });
        }).WithName("StartCourseEnrollment");

        // ============================================================
        // Genomför kurs (med resultat)
        // ============================================================

        utbildning.MapPost("/anmalan/{id:guid}/genomfor", async (Guid id, GenomforCourseRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var enrollment = await db.CourseEnrollments.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (enrollment is null) return Results.NotFound();

            try
            {
                enrollment.Genomfor(req.Resultat, req.GiltighetManader);
                await db.SaveChangesAsync(ct);

                return Results.Ok(new
                {
                    enrollment.Id,
                    Progress = enrollment.Progress.ToString(),
                    enrollment.Resultat, enrollment.Godkand,
                    enrollment.GenomfordVid, enrollment.GiltigTill
                });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("CompleteCourseEnrollment");

        // ============================================================
        // Lista lärstigar
        // ============================================================

        utbildning.MapGet("/larstigar", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var paths = await db.LearningPaths
                .Include(p => p.Steg)
                .OrderBy(p => p.Namn)
                .ToListAsync(ct);

            return Results.Ok(paths.Select(p => new
            {
                p.Id, p.Namn, p.Beskrivning, p.RollNamn,
                AntalSteg = p.Steg.Count,
                Steg = p.Steg.OrderBy(s => s.Ordning).Select(s => new
                {
                    s.Id, s.CourseId, s.Ordning, s.Obligatorisk
                })
            }));
        }).WithName("ListLearningPaths");

        // ============================================================
        // Skapa lärstig
        // ============================================================

        utbildning.MapPost("/larstig", async (CreateLearningPathRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var path = LearningPath.Skapa(req.Namn, req.Beskrivning, req.RollNamn);

            if (req.Steg is not null)
            {
                foreach (var steg in req.Steg)
                    path.LaggTillSteg(steg.CourseId, steg.Ordning, steg.Obligatorisk);
            }

            await db.LearningPaths.AddAsync(path, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/utbildning/larstig/{path.Id}", new
            {
                path.Id, path.Namn, path.Beskrivning, path.RollNamn,
                AntalSteg = path.Steg.Count
            });
        }).WithName("CreateLearningPath");

        return app;
    }
}

// Request DTOs
record CreateCourseRequest(string Namn, string Beskrivning, string Format, int LangdMinuter, bool Obligatorisk, string? Kategori = null, int? GiltighetManader = null, int MaxDeltagare = 0);
record CreateCourseEnrollmentRequest(Guid AnstallId, Guid CourseId);
record GenomforCourseRequest(int Resultat, int? GiltighetManader = null);
record CreateLearningPathRequest(string Namn, string Beskrivning, string? RollNamn = null, List<LearningPathStepDto>? Steg = null);
record LearningPathStepDto(Guid CourseId, int Ordning, bool Obligatorisk = true);
