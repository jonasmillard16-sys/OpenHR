using RegionHR.Platform.Domain;
using Xunit;

namespace RegionHR.Platform.Tests;

public class EventSubscriptionTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var sub = EventSubscription.Skapa(
            "Test Hook",
            "https://example.com/webhook",
            "my-secret",
            """["employee.created","employee.updated"]""");

        Assert.NotEqual(Guid.Empty, sub.Id);
        Assert.Equal("Test Hook", sub.Namn);
        Assert.Equal("https://example.com/webhook", sub.Url);
        Assert.Equal("my-secret", sub.HemligNyckel);
        Assert.Contains("employee.created", sub.EventFilter);
        Assert.Equal(EventSubscriptionStatus.Active, sub.Status);
        Assert.Equal(0, sub.KonsekutivaMisslyckanden);
    }

    [Fact]
    public void Pausa_SetsStatusToPaused()
    {
        var sub = EventSubscription.Skapa("Test", "https://example.com", "secret");
        Assert.Equal(EventSubscriptionStatus.Active, sub.Status);

        sub.Pausa();
        Assert.Equal(EventSubscriptionStatus.Paused, sub.Status);
    }

    [Fact]
    public void Aktivera_SetsStatusToActiveAndResetsFailures()
    {
        var sub = EventSubscription.Skapa("Test", "https://example.com", "secret");
        sub.Pausa();
        sub.OkaMisslyckanden();
        sub.OkaMisslyckanden();

        sub.Aktivera();
        Assert.Equal(EventSubscriptionStatus.Active, sub.Status);
        Assert.Equal(0, sub.KonsekutivaMisslyckanden);
    }

    [Fact]
    public void MarkeraSomFailed_SetsStatusToFailed()
    {
        var sub = EventSubscription.Skapa("Test", "https://example.com", "secret");
        sub.MarkeraSomFailed();
        Assert.Equal(EventSubscriptionStatus.Failed, sub.Status);
    }

    [Fact]
    public void OkaMisslyckanden_IncrementsCounter()
    {
        var sub = EventSubscription.Skapa("Test", "https://example.com", "secret");
        sub.OkaMisslyckanden();
        Assert.Equal(1, sub.KonsekutivaMisslyckanden);
        sub.OkaMisslyckanden();
        Assert.Equal(2, sub.KonsekutivaMisslyckanden);
    }

    [Fact]
    public void OkaMisslyckanden_AutoPausesAfter10Failures()
    {
        var sub = EventSubscription.Skapa("Test", "https://example.com", "secret");
        for (int i = 0; i < 10; i++)
            sub.OkaMisslyckanden();

        Assert.Equal(EventSubscriptionStatus.Failed, sub.Status);
        Assert.Equal(10, sub.KonsekutivaMisslyckanden);
    }

    [Fact]
    public void OkaMisslyckanden_DoesNotFailBeforeTenthFailure()
    {
        var sub = EventSubscription.Skapa("Test", "https://example.com", "secret");
        for (int i = 0; i < 9; i++)
            sub.OkaMisslyckanden();

        Assert.Equal(EventSubscriptionStatus.Active, sub.Status);
        Assert.Equal(9, sub.KonsekutivaMisslyckanden);
    }

    [Fact]
    public void MatcharEventTyp_EmptyFilterMatchesAll()
    {
        var sub = EventSubscription.Skapa("Test", "https://example.com", "secret");
        Assert.True(sub.MatcharEventTyp("employee.created"));
        Assert.True(sub.MatcharEventTyp("anything.at.all"));
    }

    [Fact]
    public void MatcharEventTyp_SpecificFilterMatchesCorrectTypes()
    {
        var sub = EventSubscription.Skapa("Test", "https://example.com", "secret",
            """["employee.created","payroll.run.completed"]""");

        Assert.True(sub.MatcharEventTyp("employee.created"));
        Assert.True(sub.MatcharEventTyp("payroll.run.completed"));
        Assert.False(sub.MatcharEventTyp("employee.updated"));
        Assert.False(sub.MatcharEventTyp("leave.request.created"));
    }

    [Fact]
    public void StatusTransition_ActiveToPausedToActiveToFailed()
    {
        var sub = EventSubscription.Skapa("Test", "https://example.com", "secret");
        Assert.Equal(EventSubscriptionStatus.Active, sub.Status);

        sub.Pausa();
        Assert.Equal(EventSubscriptionStatus.Paused, sub.Status);

        sub.Aktivera();
        Assert.Equal(EventSubscriptionStatus.Active, sub.Status);

        sub.MarkeraSomFailed();
        Assert.Equal(EventSubscriptionStatus.Failed, sub.Status);
    }
}
