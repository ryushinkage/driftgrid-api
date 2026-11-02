using Driftgrid.Domain.Exceptions;

namespace Driftgrid.Domain.Entities;

public sealed class PilotProfile
{
    private PilotProfile() { } // ef Core

    public PilotProfile(
        Guid userId,
        string callsign,
        string? bio = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException(
                "UserId cannot be empty.");
        }

        UserId = userId;

        UpdateDetails(callsign, bio);
        CreatedAtUtc = UpdatedAtUtc;
    }

    public Guid UserId { get; private set; }

    public string Callsign { get; private set; } =
        string.Empty;

    public string? Bio { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void UpdateDetails(
        string callsign,
        string? bio)
    {
        if (string.IsNullOrWhiteSpace(callsign))
        {
            throw new DomainValidationException(
                "Callsign is required.");
        }

        var normalizedCallsign = callsign.Trim();

        if (normalizedCallsign.Length > 40)
        {
            throw new DomainValidationException(
                "Callsign cannot exceed 40 characters.");
        }

        var normalizedBio = string.IsNullOrWhiteSpace(bio)
            ? null
            : bio.Trim();

        if (normalizedBio?.Length > 500)
        {
            throw new DomainValidationException(
                "Bio cannot exceed 500 characters.");
        }

        Callsign = normalizedCallsign;
        Bio = normalizedBio;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}