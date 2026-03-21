using RegionHR.Performance.Domain;
using Xunit;

namespace RegionHR.Performance.Tests;

public class OneOnOneMeetingTests
{
    private static OneOnOneMeeting CreateMeeting() =>
        OneOnOneMeeting.Skapa(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7), "Uppföljning");

    [Fact]
    public void Skapa_SetsDefaults()
    {
        var chefId = Guid.NewGuid();
        var anstallId = Guid.NewGuid();
        var datum = DateTime.UtcNow.AddDays(7);

        var meeting = OneOnOneMeeting.Skapa(chefId, anstallId, datum, "Agenda");

        Assert.NotEqual(Guid.Empty, meeting.Id);
        Assert.Equal(chefId, meeting.ChefId);
        Assert.Equal(anstallId, meeting.AnstallId);
        Assert.Equal(datum, meeting.Datum);
        Assert.Equal("Agenda", meeting.Agenda);
        Assert.Equal(MeetingStatus.Scheduled, meeting.Status);
        Assert.Null(meeting.Anteckningar);
        Assert.Equal("[]", meeting.AtgardsLista);
    }

    [Fact]
    public void Skapa_WithoutAgenda_SetsNull()
    {
        var meeting = OneOnOneMeeting.Skapa(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        Assert.Null(meeting.Agenda);
    }

    [Fact]
    public void Skapa_WithEmptyChefId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            OneOnOneMeeting.Skapa(Guid.Empty, Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void Skapa_WithEmptyAnstallId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            OneOnOneMeeting.Skapa(Guid.NewGuid(), Guid.Empty, DateTime.UtcNow));
    }

    [Fact]
    public void Genomfor_SetsCompleted()
    {
        var meeting = CreateMeeting();

        meeting.Genomfor("Bra möte. Diskuterade mål.");

        Assert.Equal(MeetingStatus.Completed, meeting.Status);
        Assert.Equal("Bra möte. Diskuterade mål.", meeting.Anteckningar);
    }

    [Fact]
    public void Genomfor_WithNullAnteckningar_Throws()
    {
        var meeting = CreateMeeting();

        Assert.Throws<ArgumentNullException>(() => meeting.Genomfor(null!));
    }

    [Fact]
    public void Genomfor_WhenAlreadyCompleted_Throws()
    {
        var meeting = CreateMeeting();
        meeting.Genomfor("Anteckningar");

        Assert.Throws<InvalidOperationException>(() => meeting.Genomfor("Mer"));
    }

    [Fact]
    public void Avboka_SetsCancelled()
    {
        var meeting = CreateMeeting();

        meeting.Avboka();

        Assert.Equal(MeetingStatus.Cancelled, meeting.Status);
    }

    [Fact]
    public void Avboka_WhenCompleted_Throws()
    {
        var meeting = CreateMeeting();
        meeting.Genomfor("Klar");

        Assert.Throws<InvalidOperationException>(() => meeting.Avboka());
    }

    [Fact]
    public void Avboka_WhenAlreadyCancelled_Throws()
    {
        var meeting = CreateMeeting();
        meeting.Avboka();

        Assert.Throws<InvalidOperationException>(() => meeting.Avboka());
    }

    [Fact]
    public void SattAgenda_UpdatesAgenda()
    {
        var meeting = CreateMeeting();

        meeting.SattAgenda("Ny agenda");

        Assert.Equal("Ny agenda", meeting.Agenda);
    }
}
