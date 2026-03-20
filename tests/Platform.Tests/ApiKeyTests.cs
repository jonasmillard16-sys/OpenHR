using RegionHR.Platform.Domain;
using Xunit;

namespace RegionHR.Platform.Tests;

public class ApiKeyTests
{
    [Fact]
    public void Skapa_ReturnsEntityAndPlaintextKey()
    {
        var (entity, plaintext) = ApiKey.Skapa("Test Key", "admin");

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Test Key", entity.Namn);
        Assert.Equal("admin", entity.SkapadAv);
        Assert.True(entity.ArAktiv);
        Assert.NotNull(plaintext);
        Assert.StartsWith("ohr_", plaintext);
    }

    [Fact]
    public void Skapa_StoresHashNotPlaintext()
    {
        var (entity, plaintext) = ApiKey.Skapa("Test Key", "admin");

        Assert.NotEqual(plaintext, entity.NyckelHash);
        Assert.Equal(64, entity.NyckelHash.Length); // SHA-256 hex = 64 chars
    }

    [Fact]
    public void Skapa_PrefixIsFirst8CharsOfPlaintext()
    {
        var (entity, plaintext) = ApiKey.Skapa("Test Key", "admin");

        Assert.Equal(plaintext[..8], entity.NyckelPrefix);
    }

    [Fact]
    public void VerifieraNyckel_ReturnsTrueForCorrectKey()
    {
        var (entity, plaintext) = ApiKey.Skapa("Test Key", "admin");

        Assert.True(entity.VerifieraNyckel(plaintext));
    }

    [Fact]
    public void VerifieraNyckel_ReturnsFalseForWrongKey()
    {
        var (entity, _) = ApiKey.Skapa("Test Key", "admin");

        Assert.False(entity.VerifieraNyckel("ohr_wrongkey12345678901234567890"));
    }

    [Fact]
    public void HashKey_IsDeterministic()
    {
        var hash1 = ApiKey.HashKey("test-key-123");
        var hash2 = ApiKey.HashKey("test-key-123");

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashKey_DifferentInputsDifferentHashes()
    {
        var hash1 = ApiKey.HashKey("key-1");
        var hash2 = ApiKey.HashKey("key-2");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Inaktivera_SetsArAktivToFalse()
    {
        var (entity, _) = ApiKey.Skapa("Test Key", "admin");
        Assert.True(entity.ArAktiv);

        entity.Inaktivera();
        Assert.False(entity.ArAktiv);
    }

    [Fact]
    public void UppdateraSenastAnvand_SetsTimestamp()
    {
        var (entity, _) = ApiKey.Skapa("Test Key", "admin");
        Assert.Null(entity.SenastAnvand);

        entity.UppdateraSenastAnvand();
        Assert.NotNull(entity.SenastAnvand);
    }

    [Fact]
    public void ArGiltig_ReturnsTrueForActiveNonExpiredKey()
    {
        var (entity, _) = ApiKey.Skapa("Test Key", "admin",
            utgarDatum: DateTime.UtcNow.AddDays(30));

        Assert.True(entity.ArGiltig());
    }

    [Fact]
    public void ArGiltig_ReturnsFalseForInactiveKey()
    {
        var (entity, _) = ApiKey.Skapa("Test Key", "admin");
        entity.Inaktivera();

        Assert.False(entity.ArGiltig());
    }

    [Fact]
    public void Skapa_WithScope_StoresScope()
    {
        var (entity, _) = ApiKey.Skapa("Test Key", "admin",
            scope: """{"core":"read","payroll":"write"}""");

        Assert.Contains("core", entity.Scope);
        Assert.Contains("payroll", entity.Scope);
    }

    [Fact]
    public void Skapa_WithoutScope_DefaultsToEmptyObject()
    {
        var (entity, _) = ApiKey.Skapa("Test Key", "admin");

        Assert.Equal("{}", entity.Scope);
    }

    [Fact]
    public void Skapa_MultipleCalls_GenerateUniqueKeys()
    {
        var (entity1, key1) = ApiKey.Skapa("Key 1", "admin");
        var (entity2, key2) = ApiKey.Skapa("Key 2", "admin");

        Assert.NotEqual(key1, key2);
        Assert.NotEqual(entity1.NyckelHash, entity2.NyckelHash);
        Assert.NotEqual(entity1.Id, entity2.Id);
    }
}
