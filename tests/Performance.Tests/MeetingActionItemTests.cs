using RegionHR.Performance.Domain;
using Xunit;

namespace RegionHR.Performance.Tests;

public class MeetingActionItemTests
{
    [Fact]
    public void Skapa_SetsDefaults()
    {
        var meetingId = Guid.NewGuid();
        var ansvarig = Guid.NewGuid();
        var deadline = new DateOnly(2026, 6, 1);

        var item = MeetingActionItem.Skapa(meetingId, "Boka utbildning", ansvarig, deadline);

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(meetingId, item.MeetingId);
        Assert.Equal("Boka utbildning", item.Beskrivning);
        Assert.Equal(ansvarig, item.Ansvarig);
        Assert.Equal(deadline, item.Deadline);
        Assert.Equal(ActionItemStatus.Open, item.Status);
    }

    [Fact]
    public void Skapa_WithoutDeadline_SetsNull()
    {
        var item = MeetingActionItem.Skapa(Guid.NewGuid(), "Uppgift", Guid.NewGuid());
        Assert.Null(item.Deadline);
    }

    [Fact]
    public void Skapa_WithEmptyMeetingId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            MeetingActionItem.Skapa(Guid.Empty, "Test", Guid.NewGuid()));
    }

    [Fact]
    public void Skapa_WithEmptyBeskrivning_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            MeetingActionItem.Skapa(Guid.NewGuid(), "", Guid.NewGuid()));
    }

    [Fact]
    public void Skapa_WithEmptyAnsvarig_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            MeetingActionItem.Skapa(Guid.NewGuid(), "Test", Guid.Empty));
    }

    [Fact]
    public void Paborja_TransitionsToInProgress()
    {
        var item = MeetingActionItem.Skapa(Guid.NewGuid(), "Uppgift", Guid.NewGuid());

        item.Paborja();

        Assert.Equal(ActionItemStatus.InProgress, item.Status);
    }

    [Fact]
    public void Paborja_WhenAlreadyInProgress_Throws()
    {
        var item = MeetingActionItem.Skapa(Guid.NewGuid(), "Uppgift", Guid.NewGuid());
        item.Paborja();

        Assert.Throws<InvalidOperationException>(() => item.Paborja());
    }

    [Fact]
    public void Slutfor_FromOpen_SetsDone()
    {
        var item = MeetingActionItem.Skapa(Guid.NewGuid(), "Uppgift", Guid.NewGuid());

        item.Slutfor();

        Assert.Equal(ActionItemStatus.Done, item.Status);
    }

    [Fact]
    public void Slutfor_FromInProgress_SetsDone()
    {
        var item = MeetingActionItem.Skapa(Guid.NewGuid(), "Uppgift", Guid.NewGuid());
        item.Paborja();

        item.Slutfor();

        Assert.Equal(ActionItemStatus.Done, item.Status);
    }

    [Fact]
    public void Slutfor_WhenAlreadyDone_Throws()
    {
        var item = MeetingActionItem.Skapa(Guid.NewGuid(), "Uppgift", Guid.NewGuid());
        item.Slutfor();

        Assert.Throws<InvalidOperationException>(() => item.Slutfor());
    }
}
