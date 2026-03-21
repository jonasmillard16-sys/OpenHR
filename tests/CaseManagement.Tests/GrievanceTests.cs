using RegionHR.CaseManagement.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.CaseManagement.Tests;

public class GrievanceTests
{
    private readonly EmployeeId _anstallId = EmployeeId.New();

    [Fact]
    public void Skapa_SetsAllProperties()
    {
        // Act
        var g = Grievance.Skapa(_anstallId, GrievanceType.Arbetsmiljo,
            "Bristfällig ergonomisk utrustning", "Kommunal - Lisa");

        // Assert
        Assert.NotEqual(default, g.Id);
        Assert.Equal(_anstallId, g.AnstallId);
        Assert.Equal(GrievanceType.Arbetsmiljo, g.Typ);
        Assert.Equal("Bristfällig ergonomisk utrustning", g.Beskrivning);
        Assert.Equal("Kommunal - Lisa", g.FackligRepresentant);
        Assert.Equal(GrievanceStatus.Filed, g.Status);
        Assert.Null(g.Beslut);
        Assert.Empty(g.Utredningar);
        Assert.Empty(g.Forhandlingar);
        Assert.Empty(g.Overklaganden);
    }

    [Fact]
    public void Skapa_ThrowsOnEmptyBeskrivning()
    {
        Assert.Throws<ArgumentException>(() =>
            Grievance.Skapa(_anstallId, GrievanceType.Diskriminering, ""));
    }

    [Fact]
    public void Bekrafta_TransitionsToAcknowledged()
    {
        // Arrange
        var g = Grievance.Skapa(_anstallId, GrievanceType.FormalltKlagomal, "Test");

        // Act
        g.Bekrafta();

        // Assert
        Assert.Equal(GrievanceStatus.Acknowledged, g.Status);
    }

    [Fact]
    public void Bekrafta_ThrowsWhenNotFiled()
    {
        // Arrange
        var g = Grievance.Skapa(_anstallId, GrievanceType.FormalltKlagomal, "Test");
        g.Bekrafta();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => g.Bekrafta());
    }

    [Fact]
    public void StartaUtredning_CreatesInvestigationAndTransitions()
    {
        // Arrange
        var g = Grievance.Skapa(_anstallId, GrievanceType.Trakasseri, "Mobbning på arbetsplatsen");
        g.Bekrafta();

        // Act
        g.StartaUtredning("Karl Berg");

        // Assert
        Assert.Equal(GrievanceStatus.UnderInvestigation, g.Status);
        Assert.Single(g.Utredningar);
        Assert.Equal("Karl Berg", g.Utredningar[0].Utredare);
        Assert.Null(g.Utredningar[0].Resultat);
    }

    [Fact]
    public void StartaUtredning_ThrowsWhenNotAcknowledged()
    {
        var g = Grievance.Skapa(_anstallId, GrievanceType.Trakasseri, "Test");
        Assert.Throws<InvalidOperationException>(() => g.StartaUtredning("Utredare"));
    }

    [Fact]
    public void HallForhandling_CreatesHearingAndTransitions()
    {
        // Arrange
        var g = Grievance.Skapa(_anstallId, GrievanceType.Avtalsbrott, "Avtalsbrott i schemaplanering");
        g.Bekrafta();
        g.StartaUtredning("Anna Utredare");

        // Act
        g.HallForhandling(DateTime.UtcNow, ["HR", "Fackombud", "Chef"]);

        // Assert
        Assert.Equal(GrievanceStatus.Hearing, g.Status);
        Assert.Single(g.Forhandlingar);
    }

    [Fact]
    public void FattaBeslut_SetsBeslutAndTransitions()
    {
        // Arrange
        var g = Grievance.Skapa(_anstallId, GrievanceType.Arbetsmiljo, "Farlig arbetsmiljö");
        g.Bekrafta();
        g.StartaUtredning("Utredare");
        g.HallForhandling(DateTime.UtcNow, ["HR", "Ombud"]);

        // Act
        g.FattaBeslut("Åtgärdsplan ska tas fram inom 30 dagar. Ny utrustning beställs.");

        // Assert
        Assert.Equal(GrievanceStatus.Decision, g.Status);
        Assert.Equal("Åtgärdsplan ska tas fram inom 30 dagar. Ny utrustning beställs.", g.Beslut);
    }

    [Fact]
    public void FattaBeslut_ThrowsOnEmptyBeslut()
    {
        var g = Grievance.Skapa(_anstallId, GrievanceType.Arbetsmiljo, "Test");
        g.Bekrafta();
        g.StartaUtredning("Utredare");

        Assert.Throws<ArgumentException>(() => g.FattaBeslut(""));
    }

    [Fact]
    public void Overklaga_CreatesAppealAndTransitions()
    {
        // Arrange
        var g = Grievance.Skapa(_anstallId, GrievanceType.Diskriminering, "Diskriminering vid rekrytering");
        g.Bekrafta();
        g.StartaUtredning("Utredare");
        g.HallForhandling(DateTime.UtcNow, ["HR"]);
        g.FattaBeslut("Ingen diskriminering fastställd.");

        // Act
        g.Overklaga("Beslutet tar inte hänsyn till alla vittnesuppgifter.");

        // Assert
        Assert.Equal(GrievanceStatus.Appeal, g.Status);
        Assert.Single(g.Overklaganden);
        Assert.Equal("Beslutet tar inte hänsyn till alla vittnesuppgifter.", g.Overklaganden[0].Grund);
    }

    [Fact]
    public void Overklaga_ThrowsWhenNotDecision()
    {
        var g = Grievance.Skapa(_anstallId, GrievanceType.FormalltKlagomal, "Test");
        Assert.Throws<InvalidOperationException>(() => g.Overklaga("Grund"));
    }

    [Fact]
    public void Los_TransitionsToResolved()
    {
        // Arrange
        var g = Grievance.Skapa(_anstallId, GrievanceType.Arbetsmiljo, "Test");
        g.Bekrafta();
        g.StartaUtredning("Utredare");
        g.FattaBeslut("Åtgärder vidtagna.");

        // Act
        g.Los();

        // Assert
        Assert.Equal(GrievanceStatus.Resolved, g.Status);
    }

    [Fact]
    public void Stang_TransitionsToClosed()
    {
        // Arrange
        var g = Grievance.Skapa(_anstallId, GrievanceType.Arbetsmiljo, "Test");
        g.Bekrafta();
        g.StartaUtredning("Utredare");
        g.FattaBeslut("Åtgärder.");
        g.Los();

        // Act
        g.Stang();

        // Assert
        Assert.Equal(GrievanceStatus.Closed, g.Status);
    }

    [Fact]
    public void Stang_ThrowsWhenNotResolved()
    {
        var g = Grievance.Skapa(_anstallId, GrievanceType.FormalltKlagomal, "Test");
        Assert.Throws<InvalidOperationException>(() => g.Stang());
    }

    [Fact]
    public void FullLifecycle_FiledToClosedThroughAllSteps()
    {
        // Arrange
        var g = Grievance.Skapa(_anstallId, GrievanceType.Trakasseri, "Trakasseri på avdelningen");
        Assert.Equal(GrievanceStatus.Filed, g.Status);

        // Act — Full lifecycle
        g.Bekrafta();
        Assert.Equal(GrievanceStatus.Acknowledged, g.Status);

        g.StartaUtredning("HR-specialist");
        Assert.Equal(GrievanceStatus.UnderInvestigation, g.Status);
        Assert.Single(g.Utredningar);

        g.HallForhandling(DateTime.UtcNow, ["HR", "Fack", "Chef"]);
        Assert.Equal(GrievanceStatus.Hearing, g.Status);
        Assert.Single(g.Forhandlingar);

        g.FattaBeslut("Varning utfärdad till förövaren.");
        Assert.Equal(GrievanceStatus.Decision, g.Status);

        g.Los();
        Assert.Equal(GrievanceStatus.Resolved, g.Status);

        g.Stang();
        Assert.Equal(GrievanceStatus.Closed, g.Status);
    }

    [Fact]
    public void FullLifecycle_WithAppeal()
    {
        // Arrange
        var g = Grievance.Skapa(_anstallId, GrievanceType.Diskriminering, "Diskriminering");

        // Act
        g.Bekrafta();
        g.StartaUtredning("Utredare");
        g.HallForhandling(DateTime.UtcNow, ["HR"]);
        g.FattaBeslut("Avslås");

        g.Overklaga("Otillräcklig utredning");
        Assert.Equal(GrievanceStatus.Appeal, g.Status);

        g.Los();
        Assert.Equal(GrievanceStatus.Resolved, g.Status);

        g.Stang();
        Assert.Equal(GrievanceStatus.Closed, g.Status);
    }
}
