using RegionHR.Infrastructure.Services;
using RegionHR.Scheduling.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Scheduling.Tests;

public class ShiftBidAssignerTests
{
    private readonly OrganizationId _enhetId = OrganizationId.From(Guid.NewGuid());
    private readonly ShiftBidAssigner _assigner = new();

    private OpenShift SkapaPass(DateOnly? datum = null) =>
        OpenShift.Skapa(_enhetId, datum ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            "Dag", new TimeOnly(7, 0), new TimeOnly(15, 0));

    private ShiftBidAssigner.EmployeeInfo SkapaAnstallInfo(
        EmployeeId id, DateOnly startDatum,
        int fatigue = 0, bool harPass = false, int extraPass = 0,
        List<string>? kompetenser = null)
    {
        return new ShiftBidAssigner.EmployeeInfo
        {
            AnstallId = id,
            AnstallningsDatum = startDatum,
            FatigueScore = fatigue,
            HarPassPaDatum = harPass,
            AntalExtraPassSenaste30Dagar = extraPass,
            Kompetenser = kompetenser ?? [],
            SenastePassSlut = null
        };
    }

    [Fact]
    public void FirstComeFirstServed_EarliestBidWins()
    {
        // Arrange
        var shift = SkapaPass();
        var emp1 = EmployeeId.New();
        var emp2 = EmployeeId.New();

        var bud1 = ShiftBid.Skapa(shift.Id, emp1, 1, "Tidig");
        var bud2 = ShiftBid.Skapa(shift.Id, emp2, 1, "Sen");

        var anstallda = new[]
        {
            SkapaAnstallInfo(emp1, new DateOnly(2020, 1, 1)),
            SkapaAnstallInfo(emp2, new DateOnly(2015, 1, 1))
        };

        // Act
        var result = _assigner.Tilldela(shift, [bud1, bud2], "FirstComeFirstServed", anstallda);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(emp1, result.VinnareId);
        Assert.Equal("FirstComeFirstServed", result.Metod);
        Assert.Equal(ShiftBidStatus.Accepted, bud1.Status);
        Assert.Equal(ShiftBidStatus.Rejected, bud2.Status);
        Assert.Equal(OpenShiftStatus.Assigned, shift.Status);
    }

    [Fact]
    public void Seniority_MostSeniorWins()
    {
        // Arrange
        var shift = SkapaPass();
        var junior = EmployeeId.New();
        var senior = EmployeeId.New();

        var bud1 = ShiftBid.Skapa(shift.Id, junior, 1);
        var bud2 = ShiftBid.Skapa(shift.Id, senior, 1);

        var anstallda = new[]
        {
            SkapaAnstallInfo(junior, new DateOnly(2023, 1, 1)),
            SkapaAnstallInfo(senior, new DateOnly(2010, 6, 15))
        };

        // Act
        var result = _assigner.Tilldela(shift, [bud1, bud2], "Seniority", anstallda);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(senior, result.VinnareId);
        Assert.Contains("senioritet", result.Motivering, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Kompetens_BestSkillMatchWins()
    {
        // Arrange
        var shift = SkapaPass();
        var emp1 = EmployeeId.New();
        var emp2 = EmployeeId.New();

        var bud1 = ShiftBid.Skapa(shift.Id, emp1, 1);
        var bud2 = ShiftBid.Skapa(shift.Id, emp2, 1);

        var anstallda = new[]
        {
            SkapaAnstallInfo(emp1, new DateOnly(2020, 1, 1), kompetenser: ["HLR"]),
            SkapaAnstallInfo(emp2, new DateOnly(2020, 1, 1), kompetenser: ["HLR", "IVA", "Lakemedel"])
        };

        // Act
        var result = _assigner.Tilldela(shift, [bud1, bud2], "Kompetens", anstallda,
            kravdaKompetenser: ["HLR", "IVA"]);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(emp2, result.VinnareId);
        Assert.Contains("2/2", result.Motivering);
    }

    [Fact]
    public void Rotation_LeastExtraShiftsWins()
    {
        // Arrange
        var shift = SkapaPass();
        var emp1 = EmployeeId.New();
        var emp2 = EmployeeId.New();

        var bud1 = ShiftBid.Skapa(shift.Id, emp1, 1);
        var bud2 = ShiftBid.Skapa(shift.Id, emp2, 1);

        var anstallda = new[]
        {
            SkapaAnstallInfo(emp1, new DateOnly(2020, 1, 1), extraPass: 5),
            SkapaAnstallInfo(emp2, new DateOnly(2020, 1, 1), extraPass: 1)
        };

        // Act
        var result = _assigner.Tilldela(shift, [bud1, bud2], "Rotation", anstallda);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(emp2, result.VinnareId);
        Assert.Contains("rotation", result.Motivering, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FiltersOut_HighFatigue()
    {
        // Arrange
        var shift = SkapaPass();
        var emp1 = EmployeeId.New();

        var bud1 = ShiftBid.Skapa(shift.Id, emp1, 1);

        var anstallda = new[]
        {
            SkapaAnstallInfo(emp1, new DateOnly(2020, 1, 1), fatigue: 75)
        };

        // Act
        var result = _assigner.Tilldela(shift, [bud1], "FirstComeFirstServed", anstallda);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("uppfyller kraven", result.Motivering);
    }

    [Fact]
    public void FiltersOut_AlreadyScheduled()
    {
        // Arrange
        var shift = SkapaPass();
        var emp1 = EmployeeId.New();

        var bud1 = ShiftBid.Skapa(shift.Id, emp1, 1);

        var anstallda = new[]
        {
            SkapaAnstallInfo(emp1, new DateOnly(2020, 1, 1), harPass: true)
        };

        // Act
        var result = _assigner.Tilldela(shift, [bud1], "FirstComeFirstServed", anstallda);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void FiltersOut_InsufficientRest_ATL()
    {
        // Arrange
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var shift = SkapaPass(tomorrow);
        var emp1 = EmployeeId.New();

        var bud1 = ShiftBid.Skapa(shift.Id, emp1, 1);

        // Last shift ended at 02:00 today — only 5 hours rest to 07:00 tomorrow
        var anstallda = new[]
        {
            new ShiftBidAssigner.EmployeeInfo
            {
                AnstallId = emp1,
                AnstallningsDatum = new DateOnly(2020, 1, 1),
                FatigueScore = 30,
                HarPassPaDatum = false,
                AntalExtraPassSenaste30Dagar = 0,
                Kompetenser = [],
                SenastePassSlut = tomorrow.ToDateTime(new TimeOnly(2, 0)) // 02:00 same day as shift
            }
        };

        // Act
        var result = _assigner.Tilldela(shift, [bud1], "FirstComeFirstServed", anstallda);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("uppfyller kraven", result.Motivering);
    }

    [Fact]
    public void NoBids_ReturnsFailed()
    {
        // Arrange
        var shift = SkapaPass();

        // Act
        var result = _assigner.Tilldela(shift, [], "FirstComeFirstServed", []);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Inga bud", result.Motivering);
    }

    [Fact]
    public void UnknownMethod_ThrowsArgumentException()
    {
        // Arrange
        var shift = SkapaPass();
        var emp = EmployeeId.New();
        var bud = ShiftBid.Skapa(shift.Id, emp);
        var anstallda = new[] { SkapaAnstallInfo(emp, new DateOnly(2020, 1, 1)) };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _assigner.Tilldela(shift, [bud], "OkandMetod", anstallda));
    }
}
