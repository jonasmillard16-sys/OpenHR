using System.Text.Json;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.Kronofogden;

/// <summary>
/// Adapter mot Kronofogdemyndigheten.
/// Hanterar löneutmätningsbeslut och bekräftelse av gjorda avdrag.
/// I produktion: SFTP-baserad filöverföring enligt Kronofogdens spec.
/// </summary>
public sealed class KronofogdenAdapter : IIntegrationAdapter
{
    public string SystemName => "Kronofogden";

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "HanteraUtmatningsbeslut" => await HanteraUtmatningsbeslut(request, ct),
            "BekraftaAvdrag" => await BekraftaAvdrag(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    /// <summary>
    /// Tar emot och registrerar löneutmätningsbeslut från Kronofogden.
    /// Arbetsgivaren är skyldig att göra avdrag från lön enligt beslutet.
    /// </summary>
    private Task<IntegrationResult> HanteraUtmatningsbeslut(IntegrationRequest request, CancellationToken ct)
    {
        Utmatningsbeslut? beslut;
        try
        {
            beslut = request.Payload is Utmatningsbeslut u
                ? u
                : JsonSerializer.Deserialize<Utmatningsbeslut>(request.Payload?.ToString() ?? "");
        }
        catch
        {
            return Task.FromResult(new IntegrationResult(false, "Ogiltig payload för utmätningsbeslut"));
        }

        if (beslut is null)
            return Task.FromResult(new IntegrationResult(false, "Utmätningsbeslut saknas"));

        if (string.IsNullOrWhiteSpace(beslut.Personnummer))
            return Task.FromResult(new IntegrationResult(false, "Personnummer saknas i utmätningsbeslut"));

        if (beslut.MaxBelopp <= 0)
            return Task.FromResult(new IntegrationResult(false, "MaxBelopp måste vara positivt"));

        // Registrera beslutet internt. I produktion: spara i databas och
        // koppla till löneberäkningsmodulen för automatiskt avdrag.
        var registreringsId = $"KF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}";

        return Task.FromResult(new IntegrationResult(
            true,
            $"Utmätningsbeslut registrerat ({registreringsId}). MaxBelopp: {beslut.MaxBelopp:N2} SEK, Förbehållsbelopp: {beslut.ForbehallsBelopp:N2} SEK",
            new { RegistreringsId = registreringsId, beslut.Personnummer, beslut.MaxBelopp, beslut.ForbehallsBelopp }));
    }

    /// <summary>
    /// Bekräftar genomfört löneavdrag till Kronofogden.
    /// Skickas månadsvis efter lönekörning.
    /// </summary>
    private Task<IntegrationResult> BekraftaAvdrag(IntegrationRequest request, CancellationToken ct)
    {
        Avdragsbekraftelse? bekraftelse;
        try
        {
            bekraftelse = request.Payload is Avdragsbekraftelse a
                ? a
                : JsonSerializer.Deserialize<Avdragsbekraftelse>(request.Payload?.ToString() ?? "");
        }
        catch
        {
            return Task.FromResult(new IntegrationResult(false, "Ogiltig payload för avdragsbekräftelse"));
        }

        if (bekraftelse is null)
            return Task.FromResult(new IntegrationResult(false, "Avdragsbekräftelse saknas"));

        if (bekraftelse.AvdragetBelopp < 0)
            return Task.FromResult(new IntegrationResult(false, "Avdraget belopp kan inte vara negativt"));

        // I produktion: skicka bekräftelsefil via SFTP till Kronofogden
        return Task.FromResult(new IntegrationResult(
            true,
            $"Avdragsbekräftelse skickad för period {bekraftelse.Period}. Avdraget belopp: {bekraftelse.AvdragetBelopp:N2} SEK",
            bekraftelse));
    }

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: verifiera SFTP-anslutning till Kronofogden
        return Task.FromResult(true);
    }
}

// --- Models ---

/// <summary>
/// Löneutmätningsbeslut från Kronofogdemyndigheten.
/// </summary>
public sealed class Utmatningsbeslut
{
    public string Personnummer { get; set; } = string.Empty;
    public decimal MaxBelopp { get; set; }            // Max belopp att utmäta per månad
    public decimal ForbehallsBelopp { get; set; }     // Belopp den anställde ska behålla (normalbelopp)
    public DateOnly BeslutDatum { get; set; }
}

/// <summary>
/// Bekräftelse av genomfört löneavdrag till Kronofogden.
/// </summary>
public sealed class Avdragsbekraftelse
{
    public string Period { get; set; } = string.Empty;    // "YYYYMM"
    public decimal AvdragetBelopp { get; set; }
}
