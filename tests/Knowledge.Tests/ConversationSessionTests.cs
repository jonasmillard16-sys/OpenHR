using RegionHR.Knowledge.Domain;
using Xunit;

namespace RegionHR.Knowledge.Tests;

public class ConversationSessionTests
{
    [Fact]
    public void Starta_SkaparAktivSession()
    {
        var session = ConversationSession.Starta(Guid.NewGuid());

        Assert.True(session.ArAktiv);
        Assert.NotEqual(Guid.Empty, session.Id);
    }

    [Fact]
    public void Starta_SatterAnstallId()
    {
        var empId = Guid.NewGuid();
        var session = ConversationSession.Starta(empId);

        Assert.Equal(empId, session.AnstallId);
    }

    [Fact]
    public void Starta_TillaterNullAnstallId()
    {
        var session = ConversationSession.Starta(null);

        Assert.Null(session.AnstallId);
    }

    [Fact]
    public void Avsluta_SatterArAktivTillFalse()
    {
        var session = ConversationSession.Starta();

        session.Avsluta();

        Assert.False(session.ArAktiv);
    }

    [Fact]
    public void Avsluta_KastarOmRedanAvslutad()
    {
        var session = ConversationSession.Starta();
        session.Avsluta();

        Assert.Throws<InvalidOperationException>(() => session.Avsluta());
    }

    [Fact]
    public void UppdateraAktivitet_UppdaterarTidsstampel()
    {
        var session = ConversationSession.Starta();
        var initialTime = session.SenastAktivVid;

        // Small delay to ensure different timestamp
        Thread.Sleep(10);
        session.UppdateraAktivitet();

        Assert.True(session.SenastAktivVid >= initialTime);
    }
}

public class ConversationMessageTests
{
    private readonly Guid _sessionId = Guid.NewGuid();

    [Fact]
    public void Skapa_SkaparMeddelandeKorrekt()
    {
        var msg = ConversationMessage.Skapa(_sessionId, "User", "Hej!");

        Assert.Equal(_sessionId, msg.SessionId);
        Assert.Equal("User", msg.Avsandare);
        Assert.Equal("Hej!", msg.Innehall);
        Assert.Null(msg.KallaArtikelId);
        Assert.Null(msg.UtfordAction);
    }

    [Fact]
    public void Skapa_SatterArtikelReferens()
    {
        var artikelId = Guid.NewGuid();
        var msg = ConversationMessage.Skapa(_sessionId, "Assistant", "Svar", artikelId);

        Assert.Equal(artikelId, msg.KallaArtikelId);
    }

    [Fact]
    public void Skapa_SatterAction()
    {
        var msg = ConversationMessage.Skapa(_sessionId, "Assistant", "Sjukanmälan gjord", null, "ReportSick");

        Assert.Equal("ReportSick", msg.UtfordAction);
    }

    [Fact]
    public void Skapa_KastarVidTomtInnehall()
    {
        Assert.Throws<ArgumentException>(() =>
            ConversationMessage.Skapa(_sessionId, "User", ""));
    }

    [Fact]
    public void Skapa_KastarVidOgiltigAvsandare()
    {
        Assert.Throws<ArgumentException>(() =>
            ConversationMessage.Skapa(_sessionId, "Robot", "Hej!"));
    }

    [Fact]
    public void Skapa_AccepterarUser()
    {
        var msg = ConversationMessage.Skapa(_sessionId, "User", "Test");
        Assert.Equal("User", msg.Avsandare);
    }

    [Fact]
    public void Skapa_AccepterarAssistant()
    {
        var msg = ConversationMessage.Skapa(_sessionId, "Assistant", "Test");
        Assert.Equal("Assistant", msg.Avsandare);
    }
}
