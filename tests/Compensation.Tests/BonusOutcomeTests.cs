using Xunit;
using RegionHR.Compensation.Domain;

namespace RegionHR.Compensation.Tests;

public class BonusOutcomeTests
{
    [Fact]
    public void Statusflode_Pending_Calculated_Approved_Paid()
    {
        var outcome = new BonusOutcome { UtfallVarde = 105m };

        Assert.Equal(BonusOutcomeStatus.Pending, outcome.Status);

        outcome.Berakna(25000m);
        Assert.Equal(BonusOutcomeStatus.Calculated, outcome.Status);
        Assert.Equal(25000m, outcome.BeraknatBelopp);

        outcome.Godkann();
        Assert.Equal(BonusOutcomeStatus.Approved, outcome.Status);

        outcome.MarkeraSomUtbetald();
        Assert.Equal(BonusOutcomeStatus.Paid, outcome.Status);
    }

    [Fact]
    public void Berakna_fran_Calculated_kastar_exception()
    {
        var outcome = new BonusOutcome { UtfallVarde = 100m };
        outcome.Berakna(10000m);

        Assert.Throws<InvalidOperationException>(() => outcome.Berakna(20000m));
    }

    [Fact]
    public void Godkann_fran_Pending_kastar_exception()
    {
        var outcome = new BonusOutcome();

        Assert.Throws<InvalidOperationException>(() => outcome.Godkann());
    }

    [Fact]
    public void Godkann_fran_Approved_kastar_exception()
    {
        var outcome = new BonusOutcome();
        outcome.Berakna(5000m);
        outcome.Godkann();

        Assert.Throws<InvalidOperationException>(() => outcome.Godkann());
    }

    [Fact]
    public void MarkeraSomUtbetald_fran_Calculated_kastar_exception()
    {
        var outcome = new BonusOutcome();
        outcome.Berakna(5000m);

        Assert.Throws<InvalidOperationException>(() => outcome.MarkeraSomUtbetald());
    }

    [Fact]
    public void MarkeraSomUtbetald_fran_Pending_kastar_exception()
    {
        var outcome = new BonusOutcome();

        Assert.Throws<InvalidOperationException>(() => outcome.MarkeraSomUtbetald());
    }
}
