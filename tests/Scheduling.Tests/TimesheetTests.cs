using RegionHR.Scheduling.Domain;
using Xunit;

namespace RegionHR.Scheduling.Tests;

public class TimesheetTests
{
    private readonly Guid _anstallId = Guid.NewGuid();
    private readonly Guid _godkannare = Guid.NewGuid();

    [Fact]
    public void Skapa_SkaparOppenTidrapport()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);

        Assert.Equal(_anstallId, ts.AnstallId);
        Assert.Equal(2026, ts.Ar);
        Assert.Equal(3, ts.Manad);
        Assert.Equal(TimesheetStatus.Oppen, ts.Status);
        Assert.Equal(160m, ts.PlaneradeTimmar);
        Assert.Equal(0m, ts.FaktiskaTimmar);
        Assert.Equal(0m, ts.Overtid);
        Assert.Equal(-160m, ts.Avvikelse);
    }

    [Fact]
    public void RegistreraTimmar_OppenTidrapport_Lyckas()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);

        ts.RegistreraTimmar(165m, 5m);

        Assert.Equal(165m, ts.FaktiskaTimmar);
        Assert.Equal(5m, ts.Overtid);
        Assert.Equal(5m, ts.Avvikelse);
    }

    [Fact]
    public void RegistreraTimmar_InskickadTidrapport_KastarUndantag()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);
        ts.RegistreraTimmar(160m);
        ts.SkickaIn();

        Assert.Throws<InvalidOperationException>(() => ts.RegistreraTimmar(170m));
    }

    [Fact]
    public void SkickaIn_OppenTidrapport_BlirInskickad()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);
        ts.RegistreraTimmar(160m);

        ts.SkickaIn();

        Assert.Equal(TimesheetStatus.Inskickad, ts.Status);
    }

    [Fact]
    public void SkickaIn_RedanInskickad_KastarUndantag()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);
        ts.RegistreraTimmar(160m);
        ts.SkickaIn();

        Assert.Throws<InvalidOperationException>(() => ts.SkickaIn());
    }

    [Fact]
    public void Godkann_InskickadTidrapport_BlirGodkand()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);
        ts.RegistreraTimmar(160m);
        ts.SkickaIn();

        ts.Godkann(_godkannare, "Ser bra ut");

        Assert.Equal(TimesheetStatus.Godkand, ts.Status);
        Assert.Equal(_godkannare, ts.GodkandAv);
        Assert.NotNull(ts.GodkandVid);
        Assert.Equal("Ser bra ut", ts.Kommentar);
    }

    [Fact]
    public void Godkann_OppenTidrapport_KastarUndantag()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);

        Assert.Throws<InvalidOperationException>(() => ts.Godkann(_godkannare));
    }

    [Fact]
    public void Avvisa_InskickadTidrapport_BlirAvslagen()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);
        ts.RegistreraTimmar(160m);
        ts.SkickaIn();

        ts.Avvisa(_godkannare, "Felaktiga timmar");

        Assert.Equal(TimesheetStatus.Avslagen, ts.Status);
        Assert.Equal(_godkannare, ts.GodkandAv);
        Assert.NotNull(ts.GodkandVid);
        Assert.Equal("Felaktiga timmar", ts.Kommentar);
    }

    [Fact]
    public void Avvisa_OppenTidrapport_KastarUndantag()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);

        Assert.Throws<InvalidOperationException>(() => ts.Avvisa(_godkannare, "Nej"));
    }

    [Fact]
    public void AteroppnaEfterAvvisning_AvslagenTidrapport_BlirOppen()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);
        ts.RegistreraTimmar(160m);
        ts.SkickaIn();
        ts.Avvisa(_godkannare, "Felaktiga timmar");

        ts.AteroppnaEfterAvvisning();

        Assert.Equal(TimesheetStatus.Oppen, ts.Status);
        Assert.Null(ts.GodkandAv);
        Assert.Null(ts.GodkandVid);
    }

    [Fact]
    public void AteroppnaEfterAvvisning_OppenTidrapport_KastarUndantag()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);

        Assert.Throws<InvalidOperationException>(() => ts.AteroppnaEfterAvvisning());
    }

    [Fact]
    public void HeltFlode_SkapaTillGodkand()
    {
        // Skapa
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);
        Assert.Equal(TimesheetStatus.Oppen, ts.Status);

        // Registrera timmar
        ts.RegistreraTimmar(168m, 8m);
        Assert.Equal(168m, ts.FaktiskaTimmar);
        Assert.Equal(8m, ts.Overtid);
        Assert.Equal(8m, ts.Avvikelse);

        // Skicka in
        ts.SkickaIn();
        Assert.Equal(TimesheetStatus.Inskickad, ts.Status);

        // Godkänn
        ts.Godkann(_godkannare, "Övertid godkänd");
        Assert.Equal(TimesheetStatus.Godkand, ts.Status);
    }

    [Fact]
    public void HeltFlode_AvvisaOchAteroppna()
    {
        var ts = Timesheet.Skapa(_anstallId, 2026, 3, 160m);
        ts.RegistreraTimmar(160m);
        ts.SkickaIn();

        // Avvisa
        ts.Avvisa(_godkannare, "Kontrollera OB-timmar");
        Assert.Equal(TimesheetStatus.Avslagen, ts.Status);

        // Återöppna
        ts.AteroppnaEfterAvvisning();
        Assert.Equal(TimesheetStatus.Oppen, ts.Status);

        // Uppdatera och skicka in igen
        ts.RegistreraTimmar(162m, 2m);
        ts.SkickaIn();
        Assert.Equal(TimesheetStatus.Inskickad, ts.Status);

        // Godkänn denna gång
        ts.Godkann(_godkannare, "Nu stämmer det");
        Assert.Equal(TimesheetStatus.Godkand, ts.Status);
    }
}
