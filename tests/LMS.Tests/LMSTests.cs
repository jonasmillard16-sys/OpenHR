using RegionHR.LMS.Domain;
using Xunit;

namespace RegionHR.LMS.Tests;

public class CourseTests
{
    [Fact]
    public void Skapa_SatterEgenskaperKorrekt()
    {
        var course = Course.Skapa("HLR-utbildning", "Grundläggande hjärt-lungräddning", CourseFormat.Klassrum, 480, true, "Medicin", 24, 20);

        Assert.Equal("HLR-utbildning", course.Namn);
        Assert.Equal("Grundläggande hjärt-lungräddning", course.Beskrivning);
        Assert.Equal(CourseFormat.Klassrum, course.Format);
        Assert.Equal(CourseStatus.Utkast, course.Status);
        Assert.Equal(480, course.LangdMinuter);
        Assert.True(course.ArObligatorisk);
        Assert.Equal("Medicin", course.Kategori);
        Assert.Equal(24, course.GiltighetManader);
        Assert.Equal(20, course.MaxDeltagare);
        Assert.NotEqual(Guid.Empty, course.Id);
    }

    [Fact]
    public void Skapa_SatterStatusTillUtkast()
    {
        var course = Course.Skapa("Brandskydd", "Brandövning", CourseFormat.Workshop, 120, true);

        Assert.Equal(CourseStatus.Utkast, course.Status);
    }

    [Fact]
    public void Publicera_AndrarStatusTillPublicerad()
    {
        var course = Course.Skapa("Ledarskap", "Grundläggande ledarskap", CourseFormat.Blandat, 960, false);

        course.Publicera();

        Assert.Equal(CourseStatus.Publicerad, course.Status);
    }

    [Fact]
    public void Arkivera_AndrarStatusTillArkiverad()
    {
        var course = Course.Skapa("Gammal kurs", "Inte längre relevant", CourseFormat.Elearning, 60, false);
        course.Publicera();

        course.Arkivera();

        Assert.Equal(CourseStatus.Arkiverad, course.Status);
    }

    [Fact]
    public void Skapa_UtanValfriaParametrar_SatterDefaults()
    {
        var course = Course.Skapa("Enkel kurs", "Beskrivning", CourseFormat.Elearning, 30, false);

        Assert.Null(course.Kategori);
        Assert.Null(course.GiltighetManader);
        Assert.Equal(0, course.MaxDeltagare);
    }
}

public class CourseEnrollmentTests
{
    private readonly Guid _anstallId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();

    [Fact]
    public void Anmala_SatterProgressTillAnmalad()
    {
        var enrollment = CourseEnrollment.Anmala(_anstallId, _courseId);

        Assert.Equal(EnrollmentProgress.Anmalad, enrollment.Progress);
        Assert.Equal(_anstallId, enrollment.AnstallId);
        Assert.Equal(_courseId, enrollment.CourseId);
        Assert.False(enrollment.Godkand);
        Assert.Null(enrollment.Resultat);
        Assert.NotEqual(Guid.Empty, enrollment.Id);
    }

    [Fact]
    public void Paborja_SatterProgressOchTidsstampel()
    {
        var enrollment = CourseEnrollment.Anmala(_anstallId, _courseId);

        enrollment.Paborja();

        Assert.Equal(EnrollmentProgress.Paborjad, enrollment.Progress);
        Assert.NotNull(enrollment.PaborjadVid);
    }

    [Fact]
    public void Genomfor_MedGodkantResultat_SatterGenomford()
    {
        var enrollment = CourseEnrollment.Anmala(_anstallId, _courseId);
        enrollment.Paborja();

        enrollment.Genomfor(85);

        Assert.Equal(EnrollmentProgress.Genomford, enrollment.Progress);
        Assert.Equal(85, enrollment.Resultat);
        Assert.True(enrollment.Godkand);
        Assert.NotNull(enrollment.GenomfordVid);
    }

    [Fact]
    public void Genomfor_MedUnderkantResultat_SatterUnderkand()
    {
        var enrollment = CourseEnrollment.Anmala(_anstallId, _courseId);
        enrollment.Paborja();

        enrollment.Genomfor(50);

        Assert.Equal(EnrollmentProgress.Underkand, enrollment.Progress);
        Assert.Equal(50, enrollment.Resultat);
        Assert.False(enrollment.Godkand);
    }

    [Fact]
    public void Genomfor_MedExakt70_ArGodkand()
    {
        var enrollment = CourseEnrollment.Anmala(_anstallId, _courseId);
        enrollment.Paborja();

        enrollment.Genomfor(70);

        Assert.Equal(EnrollmentProgress.Genomford, enrollment.Progress);
        Assert.True(enrollment.Godkand);
    }

    [Fact]
    public void Genomfor_MedOgiltigtResultat_KastarUndantag()
    {
        var enrollment = CourseEnrollment.Anmala(_anstallId, _courseId);
        enrollment.Paborja();

        Assert.Throws<ArgumentOutOfRangeException>(() => enrollment.Genomfor(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => enrollment.Genomfor(101));
    }

    [Fact]
    public void Genomfor_MedGiltighet_SatterGiltigTill()
    {
        var enrollment = CourseEnrollment.Anmala(_anstallId, _courseId);
        enrollment.Paborja();

        enrollment.Genomfor(90, giltighetManader: 24);

        Assert.NotNull(enrollment.GiltigTill);
    }

    [Fact]
    public void Avbryt_SatterProgressTillAvbruten()
    {
        var enrollment = CourseEnrollment.Anmala(_anstallId, _courseId);
        enrollment.Paborja();

        enrollment.Avbryt();

        Assert.Equal(EnrollmentProgress.Avbruten, enrollment.Progress);
    }
}

public class LearningPathTests
{
    [Fact]
    public void Skapa_SatterEgenskaperKorrekt()
    {
        var path = LearningPath.Skapa("Nyanställd sjuksköterska", "Introduktionspaket", "Sjuksköterska");

        Assert.Equal("Nyanställd sjuksköterska", path.Namn);
        Assert.Equal("Introduktionspaket", path.Beskrivning);
        Assert.Equal("Sjuksköterska", path.RollNamn);
        Assert.Empty(path.Steg);
        Assert.NotEqual(Guid.Empty, path.Id);
    }

    [Fact]
    public void LaggTillSteg_LaggerTillStegKorrekt()
    {
        var path = LearningPath.Skapa("Onboarding", "Standardintroduktion");
        var courseId = Guid.NewGuid();

        path.LaggTillSteg(courseId, 1, true);

        Assert.Single(path.Steg);
        Assert.Equal(courseId, path.Steg[0].CourseId);
        Assert.Equal(1, path.Steg[0].Ordning);
        Assert.True(path.Steg[0].Obligatorisk);
    }

    [Fact]
    public void LaggTillSteg_FleraSteg_BevarasIOrdning()
    {
        var path = LearningPath.Skapa("Chefsprogram", "Ledarutveckling");
        var course1 = Guid.NewGuid();
        var course2 = Guid.NewGuid();
        var course3 = Guid.NewGuid();

        path.LaggTillSteg(course1, 1, true);
        path.LaggTillSteg(course2, 2, true);
        path.LaggTillSteg(course3, 3, false);

        Assert.Equal(3, path.Steg.Count);
        Assert.False(path.Steg[2].Obligatorisk);
    }

    [Fact]
    public void Skapa_UtanRollNamn_ArNull()
    {
        var path = LearningPath.Skapa("Generell utbildning", "För alla");

        Assert.Null(path.RollNamn);
    }
}
