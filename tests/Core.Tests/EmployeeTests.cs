using RegionHR.Core.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Core.Tests;

public class EmployeeTests
{
    [Fact]
    public void SkapaAnstlld_SatterKorrektvarden()
    {
        var pnr = new Personnummer("198112289874");
        var employee = Employee.Skapa(pnr, "Anna", "Svensson");

        Assert.Equal("Anna", employee.Fornamn);
        Assert.Equal("Svensson", employee.Efternamn);
        Assert.Equal("Anna Svensson", employee.FulltNamn);
        Assert.NotEqual(default, employee.Id);
    }

    [Fact]
    public void LaggTillAnstallning_SkaparAnstallning()
    {
        var pnr = new Personnummer("198112289874");
        var employee = Employee.Skapa(pnr, "Erik", "Johansson");
        var enhetId = OrganizationId.New();

        var employment = employee.LaggTillAnstallning(
            enhetId,
            EmploymentType.Tillsvidare,
            CollectiveAgreementType.AB,
            Money.SEK(35000m),
            Percentage.FullTime,
            new DateOnly(2025, 1, 1));

        Assert.Single(employee.Anstallningar);
        Assert.Equal(EmploymentType.Tillsvidare, employment.Anstallningsform);
        Assert.Equal(35000m, employment.Manadslon.Amount);
    }

    [Fact]
    public void AktivAnstallning_HittarRattAnstallning()
    {
        var pnr = new Personnummer("198112289874");
        var employee = Employee.Skapa(pnr, "Maria", "Lindgren");
        var enhetId = OrganizationId.New();

        employee.LaggTillAnstallning(
            enhetId, EmploymentType.Tillsvidare, CollectiveAgreementType.AB,
            Money.SEK(30000m), Percentage.FullTime,
            new DateOnly(2024, 1, 1));

        var aktiv = employee.AktivAnstallning(new DateOnly(2025, 3, 15));
        Assert.NotNull(aktiv);
    }

    [Fact]
    public void UppdateraSkatteuppgifter_SatterVarden()
    {
        var pnr = new Personnummer("198112289874");
        var employee = Employee.Skapa(pnr, "Lars", "Nilsson");

        employee.UppdateraSkatteuppgifter(
            skattetabell: 33, skattekolumn: 1, kommun: "Göteborg",
            kommunalSkattesats: 32.30m, harKyrkoavgift: true, kyrkoavgiftssats: 1.03m);

        Assert.Equal(33, employee.Skattetabell);
        Assert.Equal(1, employee.Skattekolumn);
        Assert.True(employee.HarKyrkoavgift);
    }

    [Fact]
    public void DomainEvent_SkapasVidNyAnstallning()
    {
        var pnr = new Personnummer("198112289874");
        var employee = Employee.Skapa(pnr, "Karin", "Berg");

        // EmployeeCreatedEvent should be raised
        Assert.Single(employee.DomainEvents);
        Assert.IsType<EmployeeCreatedEvent>(employee.DomainEvents[0]);
    }
}
