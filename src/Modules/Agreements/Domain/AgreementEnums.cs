namespace RegionHR.Agreements.Domain;

/// <summary>Pensionstyp per kollektivavtal</summary>
public enum PensionType
{
    SAFLO,      // SAF-LO (arbetare, privat sektor)
    ITP1,       // ITP1 (tjänstemän, privat sektor, premiebestämd)
    ITP2,       // ITP2 (tjänstemän, privat sektor, förmånsbestämd)
    KAPKL,      // KAP-KL (kommunal/regional, äldre)
    AKAPKR,     // AKAP-KR (kommunal/regional, nyare)
    PA16,       // PA 16 (statlig sektor)
    Custom      // Anpassad pensionsplan
}

/// <summary>Branschkategori</summary>
public enum IndustrySector
{
    KommunRegion,       // Kommun och region (SKR)
    Stat,               // Statlig sektor
    IndustriTeknik,     // Industri och teknik
    Handel,             // Handel och detaljhandel
    ITTelekom,          // IT och telekommunikation
    SjukvardPrivat,     // Privat sjukvård
    Transport,          // Transport och logistik
    HotellRestaurang,   // Hotell och restaurang
    Tjanstemannaallman, // Tjänstemannasektor allmän
    Avtalslost          // Utan kollektivavtal
}

/// <summary>Avtalsstatus</summary>
public enum AgreementStatus
{
    Aktivt,     // Gällande avtal
    Kommande,   // Avtal som ännu inte trätt i kraft
    Uppsagt,    // Uppsagt avtal i uppsägningstid
    Historiskt  // Avslutat/ersatt avtal
}
