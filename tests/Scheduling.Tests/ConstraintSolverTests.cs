using RegionHR.Scheduling.Domain;
using RegionHR.Scheduling.Optimization;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Scheduling.Tests;

public class ConstraintSolverTests
{
    [Fact]
    public void Solve_EnkelBemanning_TilldelarPass()
    {
        var solver = new ConstraintScheduleSolver();
        var problem = new ScheduleProblem
        {
            EnhetId = OrganizationId.New(),
            Period = new DateRange(new DateOnly(2025, 3, 17), new DateOnly(2025, 3, 21)),
            PassBehov =
            [
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 17),
                    PassTyp = ShiftType.Dag,
                    Start = new TimeOnly(7, 0),
                    Slut = new TimeOnly(16, 0),
                    Rast = TimeSpan.FromMinutes(60),
                    KravdaKompetenser = []
                }
            ],
            TillgangligPersonal =
            [
                new PersonalInfo
                {
                    AnstallId = EmployeeId.New(),
                    Namn = "Test Person",
                    Sysselsattningsgrad = 100,
                    Kompetenser = []
                }
            ]
        };

        var solution = solver.Solve(problem);

        Assert.True(solution.ArFullstandig);
        Assert.Single(solution.Tilldelningar);
        Assert.Empty(solution.ObemannadeBehov);
    }

    [Fact]
    public void Solve_IngenPersonal_RapporterarObemannat()
    {
        var solver = new ConstraintScheduleSolver();
        var problem = new ScheduleProblem
        {
            EnhetId = OrganizationId.New(),
            Period = new DateRange(new DateOnly(2025, 3, 17), new DateOnly(2025, 3, 17)),
            PassBehov =
            [
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 17),
                    PassTyp = ShiftType.Dag,
                    Start = new TimeOnly(7, 0),
                    Slut = new TimeOnly(16, 0),
                    Rast = TimeSpan.FromMinutes(60),
                    KravdaKompetenser = ["Sjuksköterska"]
                }
            ],
            TillgangligPersonal = []
        };

        var solution = solver.Solve(problem);

        Assert.False(solution.ArFullstandig);
        Assert.Empty(solution.Tilldelningar);
        Assert.Single(solution.ObemannadeBehov);
    }

    [Fact]
    public void Solve_KompetensKrav_MatcharRattPerson()
    {
        var solver = new ConstraintScheduleSolver();
        var sjukskoterska = EmployeeId.New();
        var underskoterska = EmployeeId.New();

        var problem = new ScheduleProblem
        {
            EnhetId = OrganizationId.New(),
            Period = new DateRange(new DateOnly(2025, 3, 17), new DateOnly(2025, 3, 17)),
            PassBehov =
            [
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 17),
                    PassTyp = ShiftType.Dag,
                    Start = new TimeOnly(7, 0),
                    Slut = new TimeOnly(16, 0),
                    Rast = TimeSpan.FromMinutes(60),
                    KravdaKompetenser = ["Sjuksköterska"]
                }
            ],
            TillgangligPersonal =
            [
                new PersonalInfo
                {
                    AnstallId = underskoterska,
                    Namn = "Undersköterska",
                    Sysselsattningsgrad = 100,
                    Kompetenser = ["Undersköterska"]
                },
                new PersonalInfo
                {
                    AnstallId = sjukskoterska,
                    Namn = "Sjuksköterska",
                    Sysselsattningsgrad = 100,
                    Kompetenser = ["Sjuksköterska", "HLR"]
                }
            ]
        };

        var solution = solver.Solve(problem);

        Assert.True(solution.ArFullstandig);
        Assert.Single(solution.Tilldelningar);
        Assert.Equal(sjukskoterska, solution.Tilldelningar[0].AnstallId);
    }

    #region ATL-integrationstester

    [Fact]
    public void Solve_RespekterarATLDygnsvila()
    {
        var solver = new ConstraintScheduleSolver();
        var anstallId = EmployeeId.New();

        // Kvällspass dag 1: 15:00-23:00, sedan dagpass dag 2: 07:00-16:00
        // Vila: 23:00 till 07:00 = 8h < 11h = bör inte tilldelas
        var problem = new ScheduleProblem
        {
            EnhetId = OrganizationId.New(),
            Period = new DateRange(new DateOnly(2025, 3, 17), new DateOnly(2025, 3, 18)),
            PassBehov =
            [
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 17),
                    PassTyp = ShiftType.Kvall,
                    Start = new TimeOnly(15, 0),
                    Slut = new TimeOnly(23, 0),
                    Rast = TimeSpan.FromMinutes(30)
                },
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 18),
                    PassTyp = ShiftType.Dag,
                    Start = new TimeOnly(7, 0),
                    Slut = new TimeOnly(16, 0),
                    Rast = TimeSpan.FromMinutes(60)
                }
            ],
            TillgangligPersonal =
            [
                new PersonalInfo
                {
                    AnstallId = anstallId,
                    Namn = "Ensam personal",
                    Sysselsattningsgrad = 100,
                    Kompetenser = []
                }
            ]
        };

        var solution = solver.Solve(problem);

        // Ska bara kunna ta ett av passen (det andra bör vara obemannat)
        Assert.Single(solution.Tilldelningar);
        Assert.Single(solution.ObemannadeBehov);
        Assert.False(solution.ArFullstandig);
    }

    [Fact]
    public void Solve_HanterarNattpassKorrekt()
    {
        var solver = new ConstraintScheduleSolver();
        var anstallId = EmployeeId.New();

        // Nattpass 22:00-06:00 (8h totalt, 30 min rast = 7.5h netto, under 8h-gräns)
        // sedan kvällspass 18:00-22:00
        // Vila: 06:00 till 18:00 = 12h > 11h = OK
        var problem = new ScheduleProblem
        {
            EnhetId = OrganizationId.New(),
            Period = new DateRange(new DateOnly(2025, 3, 17), new DateOnly(2025, 3, 18)),
            PassBehov =
            [
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 17),
                    PassTyp = ShiftType.Natt,
                    Start = new TimeOnly(22, 0),
                    Slut = new TimeOnly(6, 0),
                    Rast = TimeSpan.FromMinutes(30)
                },
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 18),
                    PassTyp = ShiftType.Kvall,
                    Start = new TimeOnly(18, 0),
                    Slut = new TimeOnly(22, 0),
                    Rast = TimeSpan.FromMinutes(0)
                }
            ],
            TillgangligPersonal =
            [
                new PersonalInfo
                {
                    AnstallId = anstallId,
                    Namn = "Nattarbetare",
                    Sysselsattningsgrad = 100,
                    Kompetenser = []
                }
            ]
        };

        var solution = solver.Solve(problem);

        // Båda passen bör kunna tilldelas (12h vila, nattpass under 8h-gräns)
        Assert.Equal(2, solution.Tilldelningar.Count);
        Assert.True(solution.ArFullstandig);
    }

    [Fact]
    public void Solve_HelgpassFordelasRattvist()
    {
        var solver = new ConstraintScheduleSolver();
        var personal1 = EmployeeId.New();
        var personal2 = EmployeeId.New();

        // Två helgpass: lördag och söndag, med 2 tillgängliga
        var problem = new ScheduleProblem
        {
            EnhetId = OrganizationId.New(),
            Period = new DateRange(new DateOnly(2025, 3, 22), new DateOnly(2025, 3, 23)),
            PassBehov =
            [
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 22), // Lördag
                    PassTyp = ShiftType.Dag,
                    Start = new TimeOnly(7, 0),
                    Slut = new TimeOnly(16, 0),
                    Rast = TimeSpan.FromMinutes(60)
                },
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 23), // Söndag
                    PassTyp = ShiftType.Dag,
                    Start = new TimeOnly(7, 0),
                    Slut = new TimeOnly(16, 0),
                    Rast = TimeSpan.FromMinutes(60)
                }
            ],
            TillgangligPersonal =
            [
                new PersonalInfo { AnstallId = personal1, Namn = "Person 1", Sysselsattningsgrad = 100, Kompetenser = [] },
                new PersonalInfo { AnstallId = personal2, Namn = "Person 2", Sysselsattningsgrad = 100, Kompetenser = [] }
            ]
        };

        var solution = solver.Solve(problem);

        Assert.True(solution.ArFullstandig);
        Assert.Equal(2, solution.Tilldelningar.Count);

        // Helgpassen bör fördelas mellan de två personerna
        var tilldeladePersoner = solution.Tilldelningar.Select(t => t.AnstallId).Distinct().ToList();
        Assert.Equal(2, tilldeladePersoner.Count);
    }

    [Fact]
    public void Solve_ObemannadeBehov_Rapporteras()
    {
        var solver = new ConstraintScheduleSolver();
        var anstallId = EmployeeId.New();

        // Fler pass än vad en person kan arbeta (> 40h/vecka)
        var passBehov = new List<StaffingRequirement>();
        for (int i = 0; i < 7; i++)
        {
            passBehov.Add(new StaffingRequirement
            {
                Datum = new DateOnly(2025, 3, 17).AddDays(i),
                PassTyp = ShiftType.Dag,
                Start = new TimeOnly(7, 0),
                Slut = new TimeOnly(16, 0),
                Rast = TimeSpan.FromMinutes(60), // 8h per dag
                KravdaKompetenser = []
            });
        }

        var problem = new ScheduleProblem
        {
            EnhetId = OrganizationId.New(),
            Period = new DateRange(new DateOnly(2025, 3, 17), new DateOnly(2025, 3, 23)),
            PassBehov = passBehov,
            TillgangligPersonal =
            [
                new PersonalInfo
                {
                    AnstallId = anstallId,
                    Namn = "Ensam personal",
                    Sysselsattningsgrad = 100,
                    Kompetenser = []
                }
            ]
        };

        var solution = solver.Solve(problem);

        // Kan max tilldela 5 x 8h = 40h, resten (2 dagar) ska vara obemannade
        Assert.False(solution.ArFullstandig);
        Assert.True(solution.ObemannadeBehov.Count > 0);
        Assert.True(solution.Tilldelningar.Count <= 5);
    }

    [Fact]
    public void Solve_AntalBehov_FlerAnStaff_RapporterarObemannat()
    {
        var solver = new ConstraintScheduleSolver();

        var problem = new ScheduleProblem
        {
            EnhetId = OrganizationId.New(),
            Period = new DateRange(new DateOnly(2025, 3, 17), new DateOnly(2025, 3, 17)),
            PassBehov =
            [
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 17),
                    PassTyp = ShiftType.Dag,
                    Start = new TimeOnly(7, 0),
                    Slut = new TimeOnly(16, 0),
                    Rast = TimeSpan.FromMinutes(60),
                    AntalBehov = 3 // Behöver 3 personer
                }
            ],
            TillgangligPersonal =
            [
                new PersonalInfo { AnstallId = EmployeeId.New(), Namn = "Person 1", Sysselsattningsgrad = 100, Kompetenser = [] },
                new PersonalInfo { AnstallId = EmployeeId.New(), Namn = "Person 2", Sysselsattningsgrad = 100, Kompetenser = [] }
            ]
        };

        var solution = solver.Solve(problem);

        // Bara 2 av 3 kan bemannas
        Assert.False(solution.ArFullstandig);
        Assert.Equal(2, solution.Tilldelningar.Count);
        Assert.Single(solution.ObemannadeBehov);
    }

    [Fact]
    public void Solve_RattviseScore_Beraknas()
    {
        var solver = new ConstraintScheduleSolver();

        var problem = new ScheduleProblem
        {
            EnhetId = OrganizationId.New(),
            Period = new DateRange(new DateOnly(2025, 3, 17), new DateOnly(2025, 3, 17)),
            PassBehov =
            [
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 17),
                    PassTyp = ShiftType.Dag,
                    Start = new TimeOnly(7, 0),
                    Slut = new TimeOnly(16, 0),
                    Rast = TimeSpan.FromMinutes(60)
                }
            ],
            TillgangligPersonal =
            [
                new PersonalInfo { AnstallId = EmployeeId.New(), Namn = "P1", Sysselsattningsgrad = 100, Kompetenser = [] },
                new PersonalInfo { AnstallId = EmployeeId.New(), Namn = "P2", Sysselsattningsgrad = 100, Kompetenser = [] }
            ]
        };

        var solution = solver.Solve(problem);

        // RattviseScore bör beräknas (0 eller positivt tal)
        Assert.True(solution.RattviseScore >= 0);
    }

    [Fact]
    public void Solve_PersonalLedig_TilldelsIntePass()
    {
        var solver = new ConstraintScheduleSolver();
        var ledigPerson = EmployeeId.New();
        var tillgangligPerson = EmployeeId.New();

        var problem = new ScheduleProblem
        {
            EnhetId = OrganizationId.New(),
            Period = new DateRange(new DateOnly(2025, 3, 17), new DateOnly(2025, 3, 17)),
            PassBehov =
            [
                new StaffingRequirement
                {
                    Datum = new DateOnly(2025, 3, 17),
                    PassTyp = ShiftType.Dag,
                    Start = new TimeOnly(7, 0),
                    Slut = new TimeOnly(16, 0),
                    Rast = TimeSpan.FromMinutes(60)
                }
            ],
            TillgangligPersonal =
            [
                new PersonalInfo
                {
                    AnstallId = ledigPerson,
                    Namn = "Ledig Person",
                    Sysselsattningsgrad = 100,
                    Kompetenser = [],
                    LedigaDagar = [new DateOnly(2025, 3, 17)]
                },
                new PersonalInfo
                {
                    AnstallId = tillgangligPerson,
                    Namn = "Tillgänglig Person",
                    Sysselsattningsgrad = 100,
                    Kompetenser = []
                }
            ]
        };

        var solution = solver.Solve(problem);

        Assert.True(solution.ArFullstandig);
        Assert.Single(solution.Tilldelningar);
        Assert.Equal(tillgangligPerson, solution.Tilldelningar[0].AnstallId);
    }

    #endregion
}
