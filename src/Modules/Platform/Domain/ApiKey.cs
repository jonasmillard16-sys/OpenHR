using System.Security.Cryptography;
using System.Text;

namespace RegionHR.Platform.Domain;

/// <summary>
/// API key for external system authentication. The plaintext key is only
/// available at creation time; we store a SHA-256 hash.
/// </summary>
public sealed class ApiKey
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = default!;
    public string NyckelHash { get; private set; } = default!;
    public string NyckelPrefix { get; private set; } = default!;
    public string Scope { get; private set; } = "{}";
    public DateTime? UtgarDatum { get; private set; }
    public string SkapadAv { get; private set; } = default!;
    public DateTime SkapadVid { get; private set; }
    public DateTime? SenastAnvand { get; private set; }
    public bool ArAktiv { get; private set; }

    private ApiKey() { }

    /// <summary>
    /// Creates a new API key. Returns tuple of (entity, plaintextKey).
    /// The plaintext key is shown once to the user and never stored.
    /// </summary>
    public static (ApiKey entity, string plaintextKey) Skapa(
        string namn,
        string skapadAv,
        string? scope = null,
        DateTime? utgarDatum = null)
    {
        var plaintext = GenerateKey();
        var hash = HashKey(plaintext);
        var prefix = plaintext[..8];

        var entity = new ApiKey
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            NyckelHash = hash,
            NyckelPrefix = prefix,
            Scope = scope ?? "{}",
            UtgarDatum = utgarDatum,
            SkapadAv = skapadAv,
            SkapadVid = DateTime.UtcNow,
            ArAktiv = true
        };

        return (entity, plaintext);
    }

    public void Inaktivera()
    {
        ArAktiv = false;
    }

    public void UppdateraSenastAnvand()
    {
        SenastAnvand = DateTime.UtcNow;
    }

    /// <summary>
    /// Verify a plaintext key against this API key's hash.
    /// </summary>
    public bool VerifieraNyckel(string plaintextKey)
    {
        return NyckelHash == HashKey(plaintextKey);
    }

    public bool ArGiltig()
    {
        if (!ArAktiv) return false;
        if (UtgarDatum.HasValue && UtgarDatum.Value < DateTime.UtcNow) return false;
        return true;
    }

    public static string HashKey(string plaintext)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plaintext));
        return Convert.ToHexStringLower(bytes);
    }

    private static string GenerateKey()
    {
        // Format: ohr_{32 random hex chars} = 36 chars total
        var randomBytes = RandomNumberGenerator.GetBytes(16);
        return $"ohr_{Convert.ToHexStringLower(randomBytes)}";
    }
}
