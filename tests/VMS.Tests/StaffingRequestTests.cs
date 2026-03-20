using Xunit;
using RegionHR.VMS.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.VMS.Tests;

public class StaffingRequestTests
{
    private static StaffingRequest SkapaDraft()
    {
        return StaffingRequest.Skapa(
            OrganizationId.New(),
            "Sjukskoterska",
            new DateOnly(2026, 4, 1),
            new DateOnly(2026, 6, 30),
            2,
            "Minst 3 ars erfarenhet inom akutsjukvard");
    }

    [Fact]
    public void Skapa_SkaparBestallning_MedDraftStatus()
    {
        var req = SkapaDraft();

        Assert.Equal("Sjukskoterska", req.Befattning);
        Assert.Equal(2, req.AntalPersoner);
        Assert.Equal(StaffingRequestStatus.Draft, req.Status);
        Assert.NotEqual(default, req.Id);
    }

    [Fact]
    public void SkickaIn_AndrarStatusTillSubmitted()
    {
        var req = SkapaDraft();

        req.SkickaIn();

        Assert.Equal(StaffingRequestStatus.Submitted, req.Status);
    }

    [Fact]
    public void SkickaIn_KastarFel_OmInteUtkast()
    {
        var req = SkapaDraft();
        req.SkickaIn();

        Assert.Throws<InvalidOperationException>(() => req.SkickaIn());
    }

    [Fact]
    public void Godkann_AndrarStatusTillApproved()
    {
        var req = SkapaDraft();
        req.SkickaIn();

        req.Godkann();

        Assert.Equal(StaffingRequestStatus.Approved, req.Status);
    }

    [Fact]
    public void Godkann_KastarFel_OmInteSubmitted()
    {
        var req = SkapaDraft();

        Assert.Throws<InvalidOperationException>(() => req.Godkann());
    }

    [Fact]
    public void Tillsatt_AndrarStatusTillFilled()
    {
        var req = SkapaDraft();
        req.SkickaIn();
        req.Godkann();

        req.Tillsatt();

        Assert.Equal(StaffingRequestStatus.Filled, req.Status);
    }

    [Fact]
    public void Tillsatt_KastarFel_OmInteApproved()
    {
        var req = SkapaDraft();
        req.SkickaIn();

        Assert.Throws<InvalidOperationException>(() => req.Tillsatt());
    }

    [Fact]
    public void Stang_AndrarStatusTillClosed()
    {
        var req = SkapaDraft();
        req.SkickaIn();
        req.Godkann();
        req.Tillsatt();

        req.Stang();

        Assert.Equal(StaffingRequestStatus.Closed, req.Status);
    }

    [Fact]
    public void Stang_KastarFel_OmRedanStangd()
    {
        var req = SkapaDraft();
        req.Stang();

        Assert.Throws<InvalidOperationException>(() => req.Stang());
    }

    [Fact]
    public void FulltWorkflow_DraftTillClosed()
    {
        var req = SkapaDraft();

        Assert.Equal(StaffingRequestStatus.Draft, req.Status);
        req.SkickaIn();
        Assert.Equal(StaffingRequestStatus.Submitted, req.Status);
        req.Godkann();
        Assert.Equal(StaffingRequestStatus.Approved, req.Status);
        req.Tillsatt();
        Assert.Equal(StaffingRequestStatus.Filled, req.Status);
        req.Stang();
        Assert.Equal(StaffingRequestStatus.Closed, req.Status);
    }
}
