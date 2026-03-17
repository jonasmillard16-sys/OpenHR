namespace RegionHR.SharedKernel.Domain;

/// <summary>Anställningsform</summary>
public enum EmploymentType
{
    Tillsvidare,        // Permanent
    Vikariat,           // Temporary substitute
    Provanstallning,    // Probationary
    SAVA,               // Allmän visstidsanställning
    Timavlonad,         // Hourly
    Sasongsanstallning  // Seasonal
}

/// <summary>Kollektivavtal</summary>
public enum CollectiveAgreementType
{
    AB,     // Allmänna bestämmelser (kommunal/regional)
    HOK,    // Huvudöverenskommelse om lön och villkor
    MBA,    // Medbestämandeavtal
    PAN,    // Pacta Avtal Nämndeman
    None    // Ingen avtalstillhörighet
}

/// <summary>Frånvarotyp</summary>
public enum AbsenceType
{
    Semester,               // Vacation
    Sjukfranvaro,           // Sick leave
    Foraldraledighet,       // Parental leave
    Tjanstledighet,         // Leave of absence
    VAB,                    // Care of sick child
    Komptid,                // Compensatory time off
    Utbildning,             // Training/education
    Fackligt,               // Union activities
    Permittering,           // Layoff
    Delpension              // Partial pension
}

/// <summary>Lönearts skattekategori</summary>
public enum TaxCategory
{
    Skattepliktig,          // Taxable
    Skattefri,              // Tax-free
    Forman,                 // Benefit in kind
    Traktamente,            // Per diem allowance
    Milersattning           // Mileage reimbursement
}

/// <summary>Organisationsenhetstyp</summary>
public enum OrganizationUnitType
{
    Region,
    Forvaltning,    // Administration/department
    Verksamhet,     // Business area
    Enhet,          // Unit
    Avdelning,      // Ward/section
    Sektion         // Section
}

/// <summary>Status för lönebearbetning</summary>
public enum PayrollRunStatus
{
    Skapad,         // Created
    Paborjad,       // In progress
    Beraknad,       // Calculated
    Granskad,       // Reviewed
    Godkand,        // Approved
    Utbetald,       // Paid
    Felaktig        // Error
}

/// <summary>Ärendestatus</summary>
public enum CaseStatus
{
    Oppnad,         // Opened
    UnderBehandling,// In progress
    VantarGodkannande, // Awaiting approval
    Godkand,        // Approved
    Avslagen,       // Rejected
    Avslutad,       // Closed
    Makulerad       // Cancelled
}

/// <summary>OB-tilläggskategori</summary>
public enum OBCategory
{
    VardagKvall,    // Weekday evening
    VardagNatt,     // Weekday night
    Helg,           // Weekend
    Storhelg,       // Major holiday
    Ingen           // None
}
