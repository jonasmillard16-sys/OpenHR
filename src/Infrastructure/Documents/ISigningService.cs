namespace RegionHR.Infrastructure.Documents;

/// <summary>
/// Abstraction for electronic signing. Implementations can use BankID,
/// FOSS signing solutions, or simple confirmation signing.
/// </summary>
public interface ISigningService
{
    Task<SigningRequest> CreateSigningRequestAsync(Guid documentId, Guid signerId, string signerName, CancellationToken ct = default);
    Task<SigningResult> CheckStatusAsync(Guid requestId, CancellationToken ct = default);
    Task<SigningResult> ConfirmSigningAsync(Guid requestId, string confirmationCode, CancellationToken ct = default);
}

public record SigningRequest(Guid Id, Guid DocumentId, Guid SignerId, string Status, DateTime CreatedAt);
public record SigningResult(Guid RequestId, string Status, DateTime? SignedAt, string? SignatureData);

/// <summary>
/// Simple confirmation-based signing (no external service required).
/// The signer confirms by entering their name.
/// </summary>
public class SimpleConfirmationSigningService : ISigningService
{
    public Task<SigningRequest> CreateSigningRequestAsync(Guid documentId, Guid signerId, string signerName, CancellationToken ct = default)
    {
        return Task.FromResult(new SigningRequest(Guid.NewGuid(), documentId, signerId, "Pending", DateTime.UtcNow));
    }

    public Task<SigningResult> CheckStatusAsync(Guid requestId, CancellationToken ct = default)
    {
        return Task.FromResult(new SigningResult(requestId, "Pending", null, null));
    }

    public Task<SigningResult> ConfirmSigningAsync(Guid requestId, string confirmationCode, CancellationToken ct = default)
    {
        return Task.FromResult(new SigningResult(requestId, "Signed", DateTime.UtcNow, $"Confirmed by: {confirmationCode}"));
    }
}
