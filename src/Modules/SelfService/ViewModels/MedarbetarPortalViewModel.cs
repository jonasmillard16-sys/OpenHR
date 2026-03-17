using RegionHR.SharedKernel.Domain;

namespace RegionHR.SelfService.ViewModels;

/// <summary>
/// Vy-modell för medarbetarportalen (startsida).
/// </summary>
public sealed class MedarbetarPortalViewModel
{
    public EmployeeId AnstallId { get; set; }
    public string Namn { get; set; } = string.Empty;
    public string Befattning { get; set; } = string.Empty;
    public string Enhet { get; set; } = string.Empty;

    // Löneinfo
    public string? SenasteLonespecPeriod { get; set; }
    public decimal? SenastNetto { get; set; }

    // Semester
    public int SemesterdagarKvar { get; set; }
    public int SemesterdagarIntjanade { get; set; }
    public int SemesterdagarSparade { get; set; }

    // Schema
    public List<SchemaPassVy> KommandPass { get; set; } = [];

    // Pågående ärenden
    public List<ArendeVy> PagaendeArenden { get; set; } = [];
}

/// <summary>
/// Vy-modell för chefsportalen.
/// </summary>
public sealed class ChefsPortalViewModel
{
    public EmployeeId ChefId { get; set; }
    public string Namn { get; set; } = string.Empty;
    public string Enhet { get; set; } = string.Empty;

    // Att-göra
    public int GodkannandenAttHandla { get; set; }
    public int SchemaLuckor { get; set; }
    public int LASAlarmeringar { get; set; }

    // Team
    public int AntalMedarbetare { get; set; }
    public int NarvarandeIdag { get; set; }
    public int FranvarandeIdag { get; set; }

    // Pågående
    public List<GodkannandeVy> VantandeGodkannanden { get; set; } = [];
    public List<BemanningsVy> Bemanningsstatus { get; set; } = [];
}

public sealed class SchemaPassVy
{
    public DateOnly Datum { get; set; }
    public string PassTyp { get; set; } = string.Empty;
    public string Tid { get; set; } = string.Empty;
    public string Enhet { get; set; } = string.Empty;
}

public sealed class ArendeVy
{
    public Guid Id { get; set; }
    public string Typ { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SkapadVid { get; set; }
}

public sealed class GodkannandeVy
{
    public Guid ArendeId { get; set; }
    public string ArendeTyp { get; set; } = string.Empty;
    public string MedarbetareNamn { get; set; } = string.Empty;
    public string Beskrivning { get; set; } = string.Empty;
    public DateTime VantarSedan { get; set; }
}

public sealed class BemanningsVy
{
    public string Enhet { get; set; } = string.Empty;
    public string TrafikljusStatus { get; set; } = "Gron"; // Gron/Gul/Rod
    public int Planerad { get; set; }
    public int Faktisk { get; set; }
    public int Luckor { get; set; }
}
